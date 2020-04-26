using RestSharp;
using System.Collections.Generic;
using System.Threading;
using ADP_DAP_LoadTest;


namespace ADP_DAP_LoadTest
{
    public class S01_MockWS
    {
        
        private static string urlPrefix = "http://localhost:8080";
        private SendRequests sr;

        public S01_MockWS(int thinkTime)
        {
            sr = new SendRequests(thinkTime);
            
        }

        // correlation variables, initialized value is empty. Will fill after getting response
        //public string empId = "";
        public static Dictionary<string, string> empId = new Dictionary<string, string>
        {
            {"empId"," "}
        };


        public void Req00()
        {
            Req req = new Req
            {
                uri = urlPrefix + "/test",
                method = Method.GET,
            
                extractText = empId,
                // looking for {"id":"10","employee_name
                // This means "find a string from the *response* to this request
                // This is just like LoadRunner correlation. 
                leftBoundary = "{\"id\":\"",
                rightBoundary = "\",\"employee_name",
            };
             sr.SendRequest(req);
        }


        public void Req01()
        {
            Req req = new Req
            {
                //useExtractTextInUri = true,
                // Example of using correlation from above in next request. 
                //uri = urlPrefix + "/employee/1", // original
                uri = urlPrefix + "/employee/" + empId["empId"], // we only need to refer to the previous request.
                method = Method.GET,
            };
            sr.SendRequest(req);
        }


        public void Req02()
        {
            Req req = new Req
            {
                uri = urlPrefix + "/create",
                method = Method.POST,
                body = "{\"name\":\"test\",\"salary\":\"123\",\"age\":\"23\"}"
            };
            sr.SendRequest(req);
        }

        public void Req03()
        {
            Req req = new Req
            {
                uri = urlPrefix + "/update/1",
                method = Method.PUT,
                body = "{\"name\":\"test\",\"salary\":\"123\",\"age\":\"23\"}"
            };
            sr.SendRequest(req);
        }

        public void Req04()
        {
            Req req = new Req
            {
                uri = urlPrefix + "/delete/2",
                method = Method.DELETE,
            };
            sr.SendRequest(req);
        }


        public void Pacing(int pacingTimeinMs)
        {
            Thread.Sleep(pacingTimeinMs);
        }


    }
}
