

using Project_5;

namespace Integ_Downlink_GroundSender.Tests
{
    [TestClass]
    public class Downlink_Integration
    {
        [TestMethod]
        public void Downlink_Provides_Data_To_Send_GroundSender_Begins_Transmitting()
        {
            //Arrange
            String fakeAddress = "192.168.1.1";
            String fakeEndpointGround = "https://httpbin.org/post";
            String fakeEndPointPassthrough = "https://httpbin.org/post";

            Queue<String> testQueue = GroundSender_Stubs.GetFakedTransmissionQuueue();
            Mutex bufferlock = new Mutex();
            GroundSender ground = new GroundSender(fakeEndpointGround,fakeEndPointPassthrough, ref testQueue, ref bufferlock);
            DownLink_MadeMockable link = new DownLink_MadeMockable(fakeAddress, fakeEndpointGround, fakeEndPointPassthrough, ref ground);

            //Act
            link.AddToQueue("{'path': 'https://httpbin.org/post'}");
            Thread.Sleep(5000);

            //Assert
            Assert.IsTrue(ground.IsBufferEmpty());
        }

        [TestMethod]
        public void Downlink_Is_Ready_For_Transmission_When_Idle()
        {
            //Arrange
            String fakeAddress = "https://httpbin.org";
            String fakeEndpointGround = "/post";
            String fakeEndPointPassthrough = "/post";
            bool test_status = false;

            Queue<String> testQueue = GroundSender_Stubs.GetFakedTransmissionQuueue();
            Mutex bufferlock = new Mutex();
            GroundSender ground = new GroundSender(fakeAddress + fakeEndpointGround, fakeAddress + fakeEndPointPassthrough, ref testQueue, ref bufferlock);
            DownLink_MadeMockable link = new DownLink_MadeMockable(fakeAddress, fakeEndpointGround, fakeEndPointPassthrough, ref ground);

            //Assert
            Assert.IsFalse(link.ReadytoTransmit(ground));

            //Act
            for (int i = 0; i < testQueue.Count; i++)
            {
                link.AddToQueue(testQueue.Dequeue());
            }
            Thread.Sleep(2000);
            test_status = link.ReadytoTransmit(ground);

            //Assert
            Assert.AreEqual(true, test_status);
            //removed breakpoint - release passing debug was failing.
        }
    }
}