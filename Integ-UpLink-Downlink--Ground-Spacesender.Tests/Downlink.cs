
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
    DownLink(String address, String passThroughEndPoint, String groundStationEndPoint)
    {
    }

    private bool ReadytoTransmit(ref GroundSender sender)
    {
        return Downlink_Stubs.ReadyToTransmit_Stub();
    }

    public bool AddToQueue(String payload)
    {
        return Downlink_Stubs.AddToQueue_Stub(payload);
    }

    public void Clear()
    {

    }
}