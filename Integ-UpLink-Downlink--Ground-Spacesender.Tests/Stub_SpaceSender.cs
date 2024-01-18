using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_5
{
    public static class Stub_SpaceSender
    {
        public static HttpResponseMessage HttpRequest_Stub()
        {

            HttpResponseMessage response = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            Console.WriteLine("Setting status code for test and sleeping for 200ms to simulate a response");
            Thread.Sleep(500);
            return response;
        }

        public static bool GetLinkStatus_Stub()
        {
            return true;
        }

        public static Queue<String> GetFakedTransmissionQuueue()
        {
            Queue<String> transmissions = new Queue<String>();
            transmissions.Enqueue("testTransmissions1");
            transmissions.Enqueue("testTransmissions2");
            transmissions.Enqueue("testTransmissions3");

            return transmissions;
        }

        public static void StartSendTransmission_Stub()
        {
            //Do nothing
        }

        public static void StartSendTransmission_Throws_OutOfMemoryException()
        {
            throw new OutOfMemoryException();
        }

        public static void StartSendTransmission_Throws_ThreadStateException()
        {
            throw new ThreadStateException();
        }
    }

    public static class Uplink_Stubs
    {
        public static bool AddToQueue_Stub(String payload)
        {
            return true;
        }

        public static String PeekAtAddress_Stub()
        {
            return "Test_Address";
        }

        public static bool ReadyToTransmit_Stub()
        {
            return true;
        }
    }
}
