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
        //https://httpbin.org/post

        /*
            private Uplink uLink = new Uplink("http://", "/UD_Ground_Receive");
            private DownLink dLink = new DownLink("http://", "/receive", "/C&DH_Receive");
         */
        public LinkController()
        {
            string[] addresses = File.ReadAllLines("ServiceIPs.txt");

            //3 is space UDlink
            string spaceAddr = "http://" + addresses[2].Split(',')[0];
            uLink = new Uplink(spaceAddr, "/post");
            logging.log("Uplink initialized with address " + spaceAddr);

            //5 is ground CDH
            string groundAddr = "http://" + addresses[4].Split(',')[0];
            dLink = new DownLink(groundAddr, "/post", "/status/200");
            logging.log("Downlink initialized with address " + groundAddr);
        }

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
            logging.log("Begin listening for requests");

            while (true)
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
            bool pingSttatus = false;

            // Jose moved this lol
            string responseString = "";
            System.IO.Stream output;
            byte[] buffer;

            logging.log(req);

            //handler for deciding how to process different requests
            switch (req.RawUrl)
            {
                case "/status/":    //endpoint for getting the status of the link

                    if (req.HttpMethod == "GET")
                    {
                        try
                        {
                            int remainingBandwidth = getBandwidth();
                            logging.log("Client requested status. Remaining Bandwidth: " + remainingBandwidth + "\n");

                            if (!(pingSttatus = uLink.getPingStatus()))
                                remainingBandwidth = 0;

                            if (!pingSttatus)
                                resetBandwidth();

                            responseString = remainingBandwidth.ToString();

                            if(pingSttatus)
                                res.StatusCode = 200;
                            else
                                res.StatusCode = 503;

                            buffer = Encoding.UTF8.GetBytes(responseString);
                        }
                        catch (OutOfBandwidthException ex)
                        {
                            logging.log("Out of bandwidth.");
                            res.StatusCode = 503; // Service Unavailable
                            res.Close();
                        }
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
                        addBandwidth(payload.Length);

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

                        addBandwidth(payload.Length);

                        logging.log("\nStart of client data:\n");
                        logging.log("\n" + payload + "\n");
                        logging.log("\nEnd of client data\n");
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

            // The commented stuff is were stuff used to be before a changed it, just in case

            // string responseString = "";
            // byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);

            buffer = System.Text.Encoding.UTF8.GetBytes(responseString);

            res.ContentLength64 = buffer.Length;
            // System.IO.Stream output = res.OutputStream;;
            output = res.OutputStream;
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
        sb.Append(" | Recieved " + req.RawUrl + " from " + req.RemoteEndPoint);
        Console.WriteLine(sb.ToString());
        File.AppendAllText("./logs.txt", sb.ToString());
        sb.Clear();
    }

    public static void log(string msg)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append(DateTime.Now + " | ");
        sb.Append(msg);
        Console.WriteLine(sb.ToString());
        File.AppendAllText("./logs.txt", sb.ToString());
    }
}