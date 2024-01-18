using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.AutoMock;
using Project_5;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading;

namespace Unit_Tests
{
    [TestClass]
    public class SpaceSenderTests
    {
        String testJsonString = "This is a json test string the contents matters not";
        String testURL = "http://192.168.1.10/SendData";
        Queue<String> queue = Stub_SpaceSender.GetFakedTransmissionQuueue();
        Mutex bufferlock = new Mutex();

        [TestMethod]
        public async Task SpaceSender_SendTransmission_StartsThreadIfNotRunning()
        {
            // Arrange
            var sender = new SpaceSender(testURL, ref queue, ref bufferlock);

            // Act
            sender.SendTransmission();

            await Task.Delay(5000); // Wait for up to 1 second

            // Assert
            Assert.IsTrue(sender.TransmissionStatus); // Check the TransmissionStatus
        }


        [TestMethod]
        public void SpaceSender_IsBufferEmpty_ReturnsFalseWhenNotEmpty()
        {
            // Arrange
            var sender = new SpaceSender(testURL, ref queue, ref bufferlock);

            // Act
            var isEmpty = sender.IsBufferEmpty();

            // Assert
            Assert.IsFalse(isEmpty);
        }

        [TestMethod]
        public async Task SpaceSender_SendPing_SetStatus_To_True_When_PingIsSent()
        {
            var sender = new SpaceSender(testURL, ref queue, ref bufferlock);
            Assert.IsFalse(sender.TransmissionStatus); 

            sender.SendPing();
            await Task.Delay(5000);
            Assert.IsTrue(sender.TransmissionStatus);
        }

        [TestMethod]
        public void SpaceSender_SendPing_Strats_PingThread()
        {
            var sender = new SpaceSender(testURL, ref queue, ref bufferlock);
            Assert.IsFalse(sender.IsRunning_Ping());

            for (int i = 0; i < 10; i++)
                sender.SendPing();
            
            Assert.AreEqual(true, sender.IsRunning_Ping());
        }
    }
}