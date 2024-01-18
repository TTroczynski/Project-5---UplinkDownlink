using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Moq.AutoMock;
using Project_5;


namespace Unit_Tests
{
    [TestClass]
    public class Downlink
    {
        String testJsonString = "This is a json test string the contents matters not";
        String address = "https://httpbin.org/post";
        String passthrough = "/post";
        String destination = "/post";
        Queue<String> queue = GroundSender_Stubs.GetFakedTransmissionQuueue();
        Mutex bufferlock = new Mutex();

        [TestMethod]
        public void Downlink_Clear_EmptiesBuffer()
        {
            //Arrange
            DownLink dlModule = new DownLink(address, passthrough, destination);

            //Act
            while (queue.Count > 0)
            {
                dlModule.AddToQueue(queue.Dequeue());
            }
            bool resultVal = dlModule.Clear();

            //Assert
            Assert.IsTrue(resultVal);

        }
    }

}
