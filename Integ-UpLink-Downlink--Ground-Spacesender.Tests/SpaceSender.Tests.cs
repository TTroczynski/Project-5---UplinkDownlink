using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Project_5;

namespace Integration_Tests_SpaceSender
{
    [TestClass]
    public class Tests_SpaceSender
    {
        [TestMethod]
        public void SpaceSender_StartTransmission_Successfully_Starts_Tranmissions()
        {
            //Arrange
            String testPayload = "testPayload";
            String testURL = "http://192.168.1.10/SendData";
            Queue<String> queue = Stub_SpaceSender.GetFakedTransmissionQuueue();
            Mutex bufferlock = new Mutex();
            var sender = new SpaceSender(testURL, ref queue, ref bufferlock);

            //Act
            bool methodReturnStatus = sender.SendTransmission(testPayload);

            //Assert
            Assert.IsTrue(methodReturnStatus);
        }

        [TestMethod]
        public void SpaceSender_StartTransmission_Starts_Tranmission_Throws_OutOfMemory_Exceptions()
        {
            //Arrange
            String testPayload = "testPayload";
            String testURL = "http://192.168.1.10/SendData";
            Queue<String> queue = Stub_SpaceSender.GetFakedTransmissionQuueue();
            Mutex bufferlock = new Mutex();
            var sender = new SpaceSender_Exception_OutOfMemory(testURL, ref queue, ref bufferlock);

            //Act
            bool testReturnValue = sender.SendTransmission(testPayload);

            //Assert
            Assert.IsFalse(testReturnValue);
        }
        [TestMethod]
        public void SpaceSender_StartTransmission_Starts_Tranmission_Throws_ThreadState_Exceptions()
        {
            //Arrange
            String testPayload = "testPayload";
            String testURL = "http://192.168.1.10/SendData";
            Queue<String> queue = Stub_SpaceSender.GetFakedTransmissionQuueue();
            Mutex bufferlock = new Mutex();
            var sender = new SpaceSender_Exceptions(testURL, ref queue, ref bufferlock);

            //Act
            bool testReturnValue = sender.SendTransmission(testPayload);

            //Assert
            Assert.IsFalse(testReturnValue);
        }

        [TestMethod]
        public void SpaceSender_StartSendThread_Changes_Transmission_State()
        {
            //Arrange
            String testPayload = "testPayload";
            String testURL = "http://192.168.1.10/SendData";
            Queue<String> queue = Stub_SpaceSender.GetFakedTransmissionQuueue();
            Mutex bufferlock = new Mutex();
            var sender = new SpaceSender(testURL, ref queue, ref bufferlock);

            //Act
            sender.SendTransmission( testPayload);
            bool testTransmissionStatus = sender.TransmissionStatus;

            //Assert
            Assert.IsTrue(testTransmissionStatus);
        }
    }
}