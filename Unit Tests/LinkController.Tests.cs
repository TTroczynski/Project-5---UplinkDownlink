using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using link;

namespace Unit_Tests
{
    [TestClass]
    public class LinkControllerTests
    {
        //[TestClass]
        //public class APITests
        //{
        //    [TestMethod]
        //    public void LinkController_Status_Get()
        //    {

        //    }
        //    [TestMethod]
        //    public void LinkController_Status_Other()
        //    {

        //    }
        //    [TestMethod]
        //    public void LinkController_Send_Post() { }
        //    [TestMethod]
        //    public void LinkController_Send_Other() { }
        //    [TestMethod]
        //    public void LinkController_Receive_Post() { }
        //    [TestMethod]
        //    public void LinkController_Receive_Other() { }
        //    [TestMethod]
        //    public void LinkController_Handle_Other()
        //    {

        //    }
        //}

        [TestClass]
        public class bandwidthTests
        {
            [TestMethod]
            public void getBandwidthTest() 
            {
                LinkController controller = new LinkController();
                Assert.AreEqual(35000, controller.getBandwidth());
            }
            [TestMethod]
            public void addBandwidthTest()
            {
                LinkController controller = new LinkController();
                controller.addBandwidth(100);

                Assert.AreEqual(34900, controller.getBandwidth());
            }
            [TestMethod]
            public void addBandwidthErrorTest()
            {
                LinkController controller = new LinkController();
                
                Assert.ThrowsException<OutOfBandwidthException>(() => controller.addBandwidth(36000));
            }
        }

    }
}
