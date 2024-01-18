using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Project_5;
using System.Text;


public class GroundSender
{
    private Queue<String> transmissionQueue;
    HttpClient client;
    private Thread? transmissionManager;
    Mutex bufferLock;
    public bool transmissionStatus { get; set; }
    Uri targetURI;

    public GroundSender(String target, ref Queue<String> payloads, ref Mutex queuelock)
    {
        bufferLock = queuelock;
        transmissionStatus = false;
        transmissionQueue = payloads;
        targetURI = new Uri(target);
        client = new HttpClient();
        client.BaseAddress = new Uri(targetURI.GetLeftPart(UriPartial.Authority));
    }

    private String PeekAtAddress()
    {
        String nextToSend;
        String? modulePath = String.Empty;
        const String PATHKEY = "path";
        Uri destination;
        String peekedAddress = String.Empty;

        bufferLock.WaitOne();
        //add condition to let thread in in sending method
        nextToSend = transmissionQueue.Peek();
        bufferLock.ReleaseMutex();

        try
        {
            JObject json = JObject.Parse(nextToSend);
            modulePath = json[PATHKEY].Value<string>();
            if (modulePath != null)
            {
                destination = new Uri(modulePath);
                peekedAddress = destination.GetLeftPart(UriPartial.Authority);
            }

            //may have to extract module ID from path to check if it belongs to CNDH or not. CNDH only receives telemetry. else, it is passthrough. ID = 3 send to smaeplace/this, ID = 2 send to smaeplace/that
        }
        catch (JsonReaderException ex)
        {
            Console.WriteLine("Failed to check target module");
        }
        catch (ArgumentNullException ex)
        {
            Console.WriteLine("malformed target address for ground");
        }
        catch (NullReferenceException ex)
        {
            Console.WriteLine("malformed target address for ground");
        }
        catch (UriFormatException ex)
        {
            Console.WriteLine("malformed target address for ground");
        }

        return peekedAddress;
    }

    private async void StartSendThread()
    {
        String? nextToSend = null;
        HttpContent? content = null;
        HttpResponseMessage? response = null;
        String addressOfDestination = String.Empty;

        transmissionStatus = true;

        while (transmissionQueue.Count > 0)
        {

            if (transmissionStatus)
            {

                addressOfDestination = PeekAtAddress();

                if (addressOfDestination.Equals(targetURI.GetLeftPart(UriPartial.Authority).ToString()))
                {
                    bufferLock.WaitOne();

                    try
                    {
                        nextToSend = transmissionQueue.Dequeue();
                    }
                    catch (InvalidOperationException ex)
                    {
                        nextToSend = null;
                    }

                    bufferLock.ReleaseMutex();
                }
            }

            if (nextToSend != null)
            {
                content = new StringContent(nextToSend, Encoding.UTF8, "application/json");

                try
                {
                    response = await client.PostAsync(targetURI.PathAndQuery, content);

                    //Http request sends json string that was dequeued
                    if (response.IsSuccessStatusCode)
                        transmissionStatus = true;
                    else
                        transmissionStatus = false;
                }
                catch (HttpRequestException ex)
                { return; }
            }
        }
    }

    public bool IsBufferEmpty()
    {
        return transmissionQueue.Count == 0;
    }

    public bool isRunning()
    {
        bool status = false;
        if (transmissionManager != null)
            status = transmissionManager.IsAlive;
        else
            status = false;
        return status;
    }

    public bool SendTransmission()
    {
        if (transmissionManager == null)
        {
            transmissionManager = new Thread(delegate ()
            {
                StartSendThread();
            });
        }

        if (!transmissionManager.IsAlive)
        {
            if (transmissionStatus)
            {
                try
                {
                    transmissionManager.Join();
                }
                catch (ThreadStateException) { }
                catch (ThreadInterruptedException) { }
            }

            try
            {
                transmissionManager = new Thread(delegate ()
                {
                    StartSendThread();
                });
                transmissionManager.Start();
                transmissionStatus = true;
            }
            catch (ThreadStateException)
            {

                return false;
            }
            catch (OutOfMemoryException)
            {
                return false;
            }
        }
        return true;
    }
}