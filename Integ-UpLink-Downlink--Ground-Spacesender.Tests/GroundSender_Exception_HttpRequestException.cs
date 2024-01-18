

using Project_5;
using System.Collections;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;

public class GroundSender_Exception_HttpRequestException
{
    private Queue<String> transmissionQueue;
    HttpClient client;
    private Thread? transmissionManager;
    Mutex bufferLock;
    public bool transmissionStatus { get; set; }
    String targetURL;


    public GroundSender_Exception_HttpRequestException(String target, ref Queue<String> payloads, ref Mutex queuelock)
    {
        bufferLock = queuelock;
        transmissionStatus = false;
        client = new HttpClient();
        transmissionQueue = payloads;
        targetURL = target;
    }

    private String PeekAtAddress()
    {
        String nextToSend = String.Empty;
        //returns "Test_Address"
#if DEBUG

        nextToSend = Downlink_Stubs.PeekAtAddress_Stub();
#else
        bufferLock.WaitOne();
        nextToSend = transmissionQueue.Peek();
        bufferLock.ReleaseMutex();
#endif

        return nextToSend;
    }

    private async void StartSendThread()
    {
        String? nextToSend = null;
        client = new HttpClient();
        HttpContent? content = null;
        HttpResponseMessage? response = null;
        String addressOfDestination = String.Empty;

        transmissionStatus = true;

        while (transmissionQueue.Count > 0)
        {
#if !DEBUG
            if (transmissionStatus)
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
                    GroundSender_Stubs.HttpRequest_Throws_HttpRequestException_Stub();
#else
                    response = await client.PostAsync(targetURL, content);
#endif
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

    public bool SendTransmission(ref String jsonData)
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