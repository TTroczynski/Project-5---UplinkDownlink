
using Project_5;
using System.Linq.Expressions;

class DownLink
{
    private const int QUEUESIZE = 10;
    private Queue<String> payloadQueue;
    private GroundSender senderPassThrough;
    private GroundSender senderGroundStation;
    private String passThroughEndPoint;
    private String passThroughAddress;
    private String groundStationAddress;
    private String groundStationEndPoint;
    Mutex bufferLock = new Mutex(false);
    public DownLink(String address, String passThroughEndPoint, String groundStationEndPoint)
    {
        payloadQueue = new Queue<String>(QUEUESIZE);
        this.passThroughAddress = address;
        this.passThroughEndPoint = passThroughEndPoint;
        this.groundStationAddress = address;
        this.groundStationEndPoint = groundStationEndPoint;
        senderGroundStation = new GroundSender(groundStationAddress + groundStationEndPoint, ref payloadQueue, ref bufferLock);
        senderPassThrough = new GroundSender(passThroughAddress + passThroughEndPoint, ref payloadQueue, ref bufferLock);
    }

    private bool ReadytoTransmit(ref GroundSender sender)
    {
        return Downlink_Stubs.ReadyToTransmit_Stub();
    }

    public bool AddToQueue(String payload)
    {
        if (payloadQueue.Count >= QUEUESIZE)
            return false;
        bufferLock.WaitOne();
        payloadQueue.Enqueue(payload);
        bufferLock.ReleaseMutex();

        if (!senderGroundStation.isRunning())
            senderGroundStation.SendTransmission();
        else if (!senderPassThrough.isRunning())
            senderPassThrough.SendTransmission();
        else
            return false;

        return true;
    }

    public void Clear()
    {

    }
}