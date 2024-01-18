using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Text;
using Project_5;

public class SpaceSender
{
    private Queue<String> transmissionQueue;
    HttpClient client;
    private Thread? transmissionManager;
    Mutex bufferLock;
    public bool TransmissionStatus { get; set; }
    Uri targetURI;
    private Thread? transmissionManager_Ping;

    public SpaceSender(String target, ref Queue<String> payloads, ref Mutex queuelock)
    {
        bufferLock = queuelock;
        TransmissionStatus = false;
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

    public bool IsBufferEmpty()
    {
        return transmissionQueue.Count == 0;
    }

    public bool IsRunning()
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
            if (TransmissionStatus)
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
                TransmissionStatus = true;
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

    private async void StartSendThread()
    {
        String? nextToSend = null;
        HttpContent? content = null;
        HttpResponseMessage? response = null;
        String addressOfDestination = String.Empty;

        TransmissionStatus = true;

        while (transmissionQueue.Count > 0)
        {

            if (TransmissionStatus)
            {

                addressOfDestination = PeekAtAddress();

                if (addressOfDestination.Equals(targetURI.GetLeftPart(UriPartial.Authority).ToString()))
                {

                    try
                    {
                        Console.WriteLine("SpaceSender attempting communication with: " + targetURI);
                        bufferLock.WaitOne();
                        Console.WriteLine("Queue locked while removing one payload");
                        nextToSend = transmissionQueue.Dequeue();
                        bufferLock.ReleaseMutex();
                        content = new StringContent(nextToSend, Encoding.UTF8, "application/json");
                        response = await client.PostAsync(targetURI.PathAndQuery, content);

                    }
                    catch (InvalidOperationException ex)
                    {
                        nextToSend = null;
                    }
                    catch (HttpRequestException ex)
                    { return; }

                }
            }

            if (response != null)
            {
                //Http request sends json string that was dequeued
                if (response.IsSuccessStatusCode)
                {
                    TransmissionStatus = true;
                    Console.WriteLine("Space Sender sent payload and got 200OK");
                }
                else
                    TransmissionStatus = false;
            }
        }
    }

    public bool IsRunning_Ping()
    {
        bool status = false;
        if (transmissionManager_Ping != null)
            status = transmissionManager_Ping.IsAlive;
        else
            status = false;
        return status;
    }

    private async void StartPingThread()
    {
        TransmissionStatus = true;
        while (true)
        {
            try
            {
                HttpResponseMessage response = await client.GetAsync(targetURI);


                if (response.IsSuccessStatusCode)
                {
                    TransmissionStatus = true;
                    Thread.Sleep(500);
                }
                else
                {
                    TransmissionStatus = false;
                    Thread.Sleep(500);
                }
            }
            catch (HttpRequestException e)
            {
                return;
            }
        }
    }

    public bool SendPing()
    {
        if (transmissionManager_Ping == null)
        {
            transmissionManager_Ping = new Thread(delegate ()
            {
                StartPingThread();
            });

        }

        if (!transmissionManager_Ping.IsAlive)
        {
            if (TransmissionStatus)
            {
                try
                {
                    transmissionManager_Ping.Join();
                }
                catch (ThreadStateException) { }
                catch (ThreadInterruptedException) { }
            }
            try
            {
                transmissionManager_Ping = new Thread(delegate ()
                {
                    StartPingThread();
                });
                transmissionManager_Ping.Start();
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