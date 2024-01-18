using System.Net;
using Project_5;

namespace link
{
    /// <summary>
    /// API Endpoints for sending and receiving data and providing a status state
    /// </summary>
    public class LinkController
    {
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

            //handler for deciding how to process different requests
            switch (req.RawUrl)
            {
                case "/status/":    //endpoint for getting the status of the link

                    if (req.HttpMethod == "GET")
                    {
                        Console.WriteLine("Client requested status");
                        //todo: integrate getter for the link status
                        res.StatusCode = 200;
                    }
                    else
                    {
                        Console.WriteLine("request sent to /status/ with wrong operation");
                        res.StatusCode = 404;
                    }
                    break;
                case "/send/":  //case for sending things to space
                    if (req.HttpMethod == "POST")
                    {
                        Console.WriteLine("Client posted message to forward to sattlite");
                        //todo: integrate message queue system and sending system
                        res.StatusCode = 200;
                    }
                    else
                    {
                        Console.WriteLine("request sent to /send/ with wrong operation");
                        res.StatusCode = 404;
                    }
                    break;
                case "/receive/": //case for receiving things from space
                    if (req.HttpMethod == "POST")
                    {
                        Console.WriteLine("Sattelite posted message to forward to ground");
                        //todo: integrate methods for passing off data
                        res.StatusCode = 200;

                    }
                    else
                    {
                        Console.WriteLine("request sent to /recieve/ with wrong operation");
                        res.StatusCode = 404;
                    }
                    break;
                default:
                    Console.WriteLine("Requst to endpoint that doesnt exist");
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
}