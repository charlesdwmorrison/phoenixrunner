using RestSharp;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;

namespace ADP_DAP_LoadTest
{
    public class SendRequests
    {
        RestClient client;
        public static int numRestClients = 0;
        public static int responseId = 0;
        private static int requestRunningRequestCount = 0;


        /// <summary>
        /// Collection to hold all the response information. 
        /// This must be accessible to both the calling method and this class, hence it is public. 
        /// </summary>
        public static ConcurrentDictionary<int, Response>
            conCurResponseDict = new ConcurrentDictionary<int, Response>();

        LogWriter writer;
        private int thinkTime;

        PerformanceViolationChecker pvc;

        private static DateTime testStartTime;

        

        /// <summary>
        /// Constructor makes sure a new client is created for every user.
        /// </summary>
        public SendRequests(int _thinkTime)
        {
            Interlocked.Increment(ref numRestClients);
            client = new RestClient();
            writer = LogWriter.Instance;
            thinkTime = _thinkTime;
            pvc = new PerformanceViolationChecker();
        }



        /// <summary>
        /// ToDo: add pacing and think time parameters here.
        /// This will process script01 sequentially for as long as the main
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="method"></param>
        /// <param name="body"></param>
        /// <param name="requests"></param>
        public void SendRequest(Req req)
        {
            var method = req.method;
            var uri = req.uri;
            var request = new RestRequest(uri, method);
            req.reqStartTime = DateTime.Now;

            if (client.CookieContainer == null)
            {
                client.CookieContainer = new System.Net.CookieContainer();
            }

            if (req.method == Method.POST)
            {
                request.AddJsonBody(req.body);
            }

            Response response = new Response();
            Stopwatch sw = Stopwatch.StartNew();

            var dtNow = DateTime.Now;
            response.requestTimeSent = dtNow;

            if (responseId == 0)
            {
                testStartTime = dtNow;
            }

            IRestResponse result = null;
            try
            {
                result = client.Execute(request, request.Method);
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                response.responseExceptionThrown = true;
                response.responseExceptionMessage = msg;
            }
            sw.Stop();

            response.responseId = responseId;
            response.responseTtlb = sw.ElapsedMilliseconds;
            response.responseTimeReceived = DateTime.Now;
            response.responseStatus = "Finished";

            conCurResponseDict.TryAdd(responseId, response);
            Interlocked.Increment(ref responseId);

            double throughPut;
            TimeSpan duration;
            duration = DateTime.Now - testStartTime;
            throughPut = responseId / duration.TotalSeconds;

            //ToDo: more accureate throughput if calcuate e.g., last 100 requests
            if (responseId > 101)
            {
                // we can get the time embeded in the request
                duration = DateTime.Now - conCurResponseDict[responseId -100].responseTimeReceived;
                throughPut = 100 / duration.TotalSeconds;

            }

            writer.WriteToLog(" RespId=" + responseId
              //+ ",\tReqCnt = " + Interlocked.Increment(ref requestRunningRequestCount).ToString()
              + ",\tTTLB=" + response.responseTtlb
              + ",\tThrds=" + numRestClients
              + ",\tRPS=" + Math.Round(throughPut,2));           

            //Note: the correlation boundaries and variable must be 
            //      in the request in order to make it easy to understand
            //       which request/response we want the correlation from.
            string leftBoundary, rightBoundary;

            if (req.useExtractText ==true)
            {
                string correlationVariable = req.correlatedValue;
                leftBoundary = req.leftBoundary;
                rightBoundary = req.rightBoundary;
                int lBIdx = result.Content.IndexOf(leftBoundary) + leftBoundary.Length;
                int rBIdx = result.Content.IndexOf(rightBoundary); // should be the length of what we are looking for. ;
                int subStrLgth = rBIdx - lBIdx;
                string extractedValue = result.Content.Substring(lBIdx, subStrLgth);

                S02_DummyRestApi.empId["empId"] = extractedValue;

            }

            Thread.Sleep(thinkTime); 


        }

    }
}
