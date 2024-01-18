using Project_5;

namespace Integ_Uplink_SpaceSender
{
    [TestClass]
    public class Uplink_Integration
    {
        [TestMethod]
        public void Uplink_Provides_Data_To_Send_SpaceSender_Begins_Transmitting()
        {
            //Arrange
            String fakeAddress = "192.168.1.1";
            String fakeEndpointSpace = "https://httpbin.org/post";

            Queue<String> testQueue = Stub_SpaceSender.GetFakedTransmissionQuueue();
            Mutex bufferlock = new Mutex();
            SpaceSender space = new SpaceSender(fakeEndpointSpace, ref testQueue, ref bufferlock);
            Uplink_MadeMockable link = new Uplink_MadeMockable(fakeAddress, fakeEndpointSpace, ref space);

            //Act
            link.AddToQueue("{'path': 'https://httpbin.org/post'}");
            Thread.Sleep(5000);

            //Assert
            Assert.IsTrue(space.IsBufferEmpty());
        }

        [TestMethod]
        public void Uplink_Is_Ready_For_Transmission_When_Idle()
        {
            //Arrange
            String fakeAddress = "https://httpbin.org";
            String fakeEndpointSpace = "/post";

            Queue<String> testQueue = Stub_SpaceSender.GetFakedTransmissionQuueue();
            Mutex bufferlock = new Mutex();
            SpaceSender space = new SpaceSender(fakeAddress, ref testQueue, ref bufferlock);
            Uplink_MadeMockable link = new Uplink_MadeMockable(fakeAddress, fakeEndpointSpace, ref space);

            //Assert
            Assert.IsFalse(link.ReadytoTransmit(ref space));

            //Act
            for (int i = 0; i < testQueue.Count; i++)
            {
                link.AddToQueue(testQueue.Dequeue());
            }

            //Assert
            Assert.IsTrue(link.ReadytoTransmit(ref space));
        }
    }
}