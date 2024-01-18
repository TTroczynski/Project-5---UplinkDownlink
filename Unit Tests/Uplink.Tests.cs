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
    public class Uplink_Tests
    {
        String testJsonString = "This is a json test string the contents matters not";
        String address = "https://httpbin.org/post";
        String destination = "/post";
        Queue<String> queue = Stub_SpaceSender.GetFakedTransmissionQuueue();
        Mutex bufferlock = new Mutex();

        [TestMethod]
        public void Uplink_Clear_EmptiesBuffer()
        {
            //Arrange
            Uplink ulModule = new Uplink(address, destination);

            //Act
            while (queue.Count > 0)
            {
                ulModule.AddToQueue(queue.Dequeue());
            }
            bool resultVal = ulModule.Clear();

            //Assert
            Assert.IsTrue(resultVal);

        }
    }
}
