using link;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Project_5;
using System.Runtime.CompilerServices;
using Xunit;

namespace Integration_Tests
{
    [TestClass]
    public class Tests
    { 
        [TestMethod]
        public void GroundSender_StartTransmission_Successfully_Starts_Tranmissions()
        {
            //Arrange
            String testPayload = "testPayload";
            String testURL = "http://192.168.1.10/SendData";
            Queue<String> queue = GroundSender_Stubs.GetFakedTransmissionQuueue();
            Mutex bufferlock = new Mutex();
            var sender = new GroundSender(testURL, ref queue, ref bufferlock);

            //Act
            bool methodReturnStatus = sender.SendTransmission(ref testPayload);

            //Assert
            Assert.IsTrue(methodReturnStatus);
        }
        [TestMethod]
        public void GroundSender_StartTransmission_Starts_Tranmission_Throws_OutOfMemory_Exceptions()
        {
            //Arrange
            String testPayload = "testPayload";
            String testURL = "http://192.168.1.10/SendData";
            Queue<String> queue = GroundSender_Stubs.GetFakedTransmissionQuueue();
            Mutex bufferlock = new Mutex();
            var sender = new GroundSender_Exception_OutOfMemory(testURL, ref queue, ref bufferlock);

            //Act
            bool testReturnValue = sender.SendTransmission(ref testPayload);

            //Assert
            Assert.IsFalse(testReturnValue);
        }
        [TestMethod]
        public void GroundSender_StartTransmission_Starts_Tranmission_Throws_ThreadState_Exceptions()
        {
            //Arrange
            String testPayload = "testPayload";
            String testURL = "http://192.168.1.10/SendData";
            Queue<String> queue = GroundSender_Stubs.GetFakedTransmissionQuueue();
            Mutex bufferlock = new Mutex();
            var sender = new GroundSender_Exception_ThreadStateException(testURL, ref queue, ref bufferlock);

            //Act
            bool testReturnValue = sender.SendTransmission(ref testPayload);
            

            //Assert
            Assert.IsFalse(testReturnValue);
        }

        [TestMethod]
        public void GroundSender_StartSendThread_Changes_Transmission_State()
        {
            //Arrange
            String testPayload = "testPayload";
            String testURL = "http://192.168.1.10/SendData";
            Queue<String> queue = GroundSender_Stubs.GetFakedTransmissionQuueue();
            Mutex bufferlock = new Mutex();
            var sender = new GroundSender(testURL, ref queue, ref bufferlock);

            //Act
            sender.SendTransmission(ref testPayload);
            bool testTransmissionStatus = sender.transmissionStatus;

            //Assert
            Assert.IsTrue(testTransmissionStatus);
        }

        public void GroundSender_StartTranmission_Starts_Transmission_Throws_HttpRequestException()
        {
            //Arrange
            String testPayload = "testPayload";
            String testURL = "http://192.168.1.10/SendData";
            Queue<String> queue = GroundSender_Stubs.GetFakedTransmissionQuueue();
            Mutex bufferlock = new Mutex();
            var sender = new GroundSender_Exception_HttpRequestException(testURL, ref queue, ref bufferlock);

            //Act
            bool testReturnValue = sender.SendTransmission(ref testPayload);


            //Assert
            Assert.IsFalse(testReturnValue);
        }
    }
}