using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Project_5;
using System.Linq.Expressions;

class Uplink_MadeMockable
{
    private const int QUEUESIZE = 10;
    private Queue<String> payloadQueue;
    private SpaceSender senderSpace;
    private String SpaceAddress;
    private String SpaceEndPoint;
    Mutex bufferLock = new Mutex(false);

    public Uplink_MadeMockable(String address, String SpaceEndPoint, ref SpaceSender space)
    {
        payloadQueue = new Queue<String>(QUEUESIZE);
        this.SpaceAddress = address;
        this.SpaceEndPoint = SpaceEndPoint;
        senderSpace = space;
    }

    public bool ReadytoTransmit(ref SpaceSender sender)
    {
        bool status = true;

        if (!sender.TransmissionStatus)
            status = false;

        return status;
    }

    public bool AddToQueue(String payload)
    {
        if (payloadQueue.Count >= QUEUESIZE)
            return false;
        bufferLock.WaitOne();
        payloadQueue.Enqueue(payload);
        bufferLock.ReleaseMutex();

        if (!senderSpace.IsRunning())
            senderSpace.SendTransmission();
        else
            return false;

        return true;
    }

    public void Clear()
    {

    }
}