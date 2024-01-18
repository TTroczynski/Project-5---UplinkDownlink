#define TEST
using Project_5;
using System.Collections;
using System.Linq.Expressions;
using System.Text;

public class SpaceSender_Exceptions
{
    private Queue<string> transmissionQueue = new Queue<string>();
    private HttpClient client = new HttpClient();
    private Thread? transmissionManager;
    Mutex bufferLock;
    public bool TransmissionStatus { get; private set; }
    private string targetURL;
    private Thread? transmissionManager_Ping;

    public SpaceSender_Exceptions(string targetURL, ref Queue<String> payloads, ref Mutex queuelock)
    {
        this.targetURL = targetURL;
        transmissionQueue = payloads;
        client = new HttpClient();
        transmissionManager = new Thread(StartSendThread);
        transmissionManager_Ping = new Thread(StartPingThread);
        bufferLock = queuelock;
    }

    private String PeekAtAddress()
    {
        String nextToSend = String.Empty;

#if DEBUG

        nextToSend = Uplink_Stubs.PeekAtAddress_Stub();
#else
        bufferLock.WaitOne();
        nextToSend = transmissionQueue.Peek();
        bufferLock.ReleaseMutex();
#endif

        return nextToSend;
    }

    public bool IsBufferEmpty()
    {
        lock (bufferLock)
        {
            return transmissionQueue.Count == 0;
        }
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

    public bool SendTransmission(string jsonData)
    {
        try
        {
            bufferLock.WaitOne();
            transmissionQueue.Enqueue(jsonData);
            bufferLock.ReleaseMutex();
        }
        catch (ApplicationException)
        {
            return false;
        }
        catch (ObjectDisposedException)
        {
            return false;
        }


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
                Stub_SpaceSender.StartSendTransmission_Throws_OutOfMemoryException();
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
        TransmissionStatus = true;
        String? nextToSend = null;
        client = new HttpClient();
        HttpContent? content = null;
        HttpResponseMessage? response = null;
        String addressOfDestination = String.Empty;

        while (transmissionQueue.Count > 0)
        {
#if !DEBUG
            if (TransmissionStatus)
            {

                addressOfDestination = PeekAtAddress();

                if (addressOfDestination.Equals(targetURL))
                {
                    bufferLock.WaitOne();

                    try
                    {
                        nextToSend = transmissionQueue.Dequeue();
                    }catch(InvalidOperationException ex)
                    {
                        nextToSend = null;
                    }
                    
                    bufferLock.ReleaseMutex();
                }
            }
#endif       
            if (nextToSend != null)
            {
                content = new StringContent(nextToSend, Encoding.UTF8, "application/json");

                try
                {
#if DEBUG
                    response = Stub_SpaceSender.HttpRequest_Stub();
#else
                    response = await client.PostAsync(targetURL, content);
#endif
                    //Http request sends json string that was dequeued
                    if (response.IsSuccessStatusCode)
                        TransmissionStatus = true;
                    else
                        TransmissionStatus = false;
                }
                catch (HttpRequestException ex)
                { return; }
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
        while (true)
        {
            try
            {
#if DEBUG
                HttpResponseMessage response = Stub_SpaceSender.HttpRequest_Stub();
#else
                HttpResponseMessage response = await client.GetAsync(targetURL);
#endif


                if (response.IsSuccessStatusCode)
                {
                    TransmissionStatus = true;
                    Thread.Sleep(1000);
                }
                else
                {
                    TransmissionStatus = false;
                    Thread.Sleep(1000);
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
                    StartSendThread();
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