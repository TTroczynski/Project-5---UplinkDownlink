
using Project_5;
using System.Linq.Expressions;

class Uplink
{
    private const int QUEUESIZE = 10;
    private Queue<String> payloadQueue;
    private SpaceSender senderPassThrough;
    private SpaceSender senderSpaceStation;
    private String passThroughEndPoint;
    private String passThroughAddress;
    private String groundStationAddress;
    private String groundStationEndPoint;
    Mutex bufferLock = new Mutex(false);
    Uplink(String address, String passThroughEndPoint, String groundStationEndPoint)
    {
        payloadQueue = new Queue<String>(QUEUESIZE);
        this.passThroughAddress = address;
        this.passThroughEndPoint = passThroughEndPoint;
        this.groundStationAddress = address;
        this.groundStationEndPoint = groundStationEndPoint;
        senderSpaceStation = new SpaceSender(groundStationAddress + groundStationEndPoint, ref payloadQueue, ref bufferLock);
        senderPassThrough = new SpaceSender(passThroughAddress + passThroughEndPoint, ref payloadQueue, ref bufferLock);
    }

    private bool ReadytoTransmit(ref SpaceSender sender)
    {
        return !senderSpaceStation.IsRunning() && !senderPassThrough.IsRunning();
    }

    public bool AddToQueue(String payload)
    {
        if (payloadQueue.Count >= QUEUESIZE)
            return false;
        bufferLock.WaitOne();
        payloadQueue.Enqueue(payload);
        bufferLock.ReleaseMutex();
        return true;
    }

    public void Clear()
    {
        bufferLock.WaitOne();
        payloadQueue.Clear();
        bufferLock.ReleaseMutex();
    }
}