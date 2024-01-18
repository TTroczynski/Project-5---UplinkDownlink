using System.Net;
using System.Text;
using Project_5;

namespace link
{
   
    /// <summary>
    /// API Endpoints for sending and receiving data and providing a status state
    /// </summary>
    public class LinkController
    {
        //link controller data
        private Mutex bandwidthLock = new Mutex();
        private int bandwidth = 35000;
        private Uplink uLink;
        private DownLink dLink;

        //bandwidth methods
        public void resetBandwidth()
        {
            bandwidthLock.WaitOne();
            bandwidth = 35000;
            bandwidthLock.ReleaseMutex();
        }
        public int getBandwidth()
        {
            return bandwidth;
        }
        public void addBandwidth(int messageSize)
        {
            if (bandwidth - messageSize > 0)
            {
                bandwidthLock.WaitOne();
                bandwidth -= messageSize;
                bandwidthLock.ReleaseMutex();
            }
            else
            {
                throw new OutOfBandwidthException(bandwidth, messageSize);
            }
        }

        //API methods

        /// <summary>
        /// Create endpoints for the API
        /// </summary>
        public void CreateEndpoints()
        {
            //list of all endpoints
            string[] endpoints = { "http://*:8080/send/", "http://*:8080/receive/", "http://*:8080/status/" };

            //create the actual listener
            HttpListener listener = new HttpListener();

            //add all endpoints to listener
            foreach (string endpoint in endpoints)
            {
                listener.Prefixes.Add(endpoint);
            }
            listener.Start();
            Console.WriteLine(listener.ToString());

            while (true)    //REWORK THIS TO BE ABLE TO BE EXITED. ALSO THIS IS UNTESTABLE BECAUSE IT IS INFINITE
            {
                //wait and recieve incoming requests.
                HttpListenerContext context = listener.GetContext();

                handleRequest(context);

            }

            //listener.Stop();
        }

        private void handleRequest(HttpListenerContext context)
        {
            HttpListenerRequest req = context.Request;
            HttpListenerResponse res = context.Response;
            
            logging.log(req);

            //handler for deciding how to process different requests
            switch (req.RawUrl)
            {
                case "/status/":    //endpoint for getting the status of the link

                    if (req.HttpMethod == "GET")
                    {
                        logging.log("Client requested status");
                        //todo: integrate getter for the link status
                        res.StatusCode = 200;
                    }
                    else
                    {
                        logging.log("request sent to /status/ with wrong operation");
                        res.StatusCode = 404;
                    }
                    break;
                case "/send/":  //case for sending things to space
                    if (req.HttpMethod == "POST")
                    {
                        logging.log("Client posted message to forward to sattlite");
                        //todo: integrate message queue system and sending system
                        res.StatusCode = 200;

                        System.IO.Stream body = req.InputStream;
                        System.Text.Encoding encoding = req.ContentEncoding;

                        System.IO.StreamReader reader = new System.IO.StreamReader(body, encoding);

                        // Convert the data to a string and display it on the console.
                        if (!req.HasEntityBody)
                        {
                            logging.log("No client data was sent with the request.");
                            break;
                        }

                        
                        string payload = reader.ReadToEnd();
                        logging.log("End of client data:");
                        logging.log("Client sent: " + payload);
                        logging.log("End of client data");
                        body.Close();
                        reader.Close();
                        uLink.AddToQueue(payload);

                        
                    }
                    else
                    {
                        logging.log("request sent to /send/ with wrong operation");
                        res.StatusCode = 404;
                    }
                    break;
                case "/receive/": //case for receiving things from space
                    if (req.HttpMethod == "POST")
                    {
                        logging.log("Sattelite posted message to forward to ground");
                        //todo: integrate methods for passing off data
                        res.StatusCode = 200;

                        System.IO.Stream body = req.InputStream;
                        System.Text.Encoding encoding = req.ContentEncoding;

                        System.IO.StreamReader reader = new System.IO.StreamReader(body, encoding);

                        // Convert the data to a string and display it on the console.
                        if (!req.HasEntityBody)
                        {
                            logging.log("No client data was sent with the request.");
                            break;
                        }

                        string payload = reader.ReadToEnd();
                        logging.log("End of client data:");
                        logging.log("Client sent: " + payload);
                        logging.log("End of client data");
                        body.Close();
                        reader.Close();
                        dLink.AddToQueue(payload);
                    }
                    else
                    {
                        logging.log("request sent to /recieve/ with wrong operation");
                        res.StatusCode = 404;
                    }
                    break;
                default:
                    logging.log("Requst to endpoint that doesnt exist");
                    res.StatusCode = 404;
                    break;
            }

            string responseString = "";

            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);

            res.ContentLength64 = buffer.Length;
            System.IO.Stream output = res.OutputStream;
            output.Write(buffer, 0, buffer.Length);
            output.Close();
        }

    }

    public class OutOfBandwidthException : Exception
    {
        public OutOfBandwidthException() { }

        public OutOfBandwidthException(int currentBandwidth, int attemptedBandwidth)
            : base(String.Format("Tried to use more than available bandwidth ", attemptedBandwidth, " of remaining ", currentBandwidth))
        {

        }
    }

}

static class logging
{
    public static void log(HttpListenerRequest req)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append(DateTime.Now);
        sb.Append(" Recieved " + req.RawUrl + " from " + req.RemoteEndPoint + '\n');
        Console.Write(sb.ToString());
        File.AppendAllText("./logs.txt", sb.ToString());
        sb.Clear();
    }

    public static void log(string msg)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append(DateTime.Now);
        sb.Append(msg);
        Console.Write(sb.ToString());
        File.AppendAllText("./logs.txt", sb.ToString());
    }
}