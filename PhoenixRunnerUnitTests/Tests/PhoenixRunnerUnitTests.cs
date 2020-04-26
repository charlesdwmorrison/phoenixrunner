using ADP_DAP_LoadTest;
using APD_DAP.UnitTests.MockWebServer;
using NUnit.Framework;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace APD_DAP_UnitTests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
            // Run the web server
            int wsCount = 0;
            if (wsCount == 0)
            {
                WebServer ws = WebServer.Instance;
                ws.Run();
            }

        }

        /// <summary>
        /// This work load is the action that each thread will perform.
        /// </summary>
        [Test]
        public void MockWorkloadDefinition()
        {
            S01_MockWS S01MWS = new S01_MockWS(0);
            for (int i = 1; i <= 10; i++)
            {               
                S01MWS.Req00();
            }
        }


        [Test]
        public async Task Mock_MultiUser_100Users()
        {
            UserController uc = new UserController();
            await Task.Run(() => uc.RampUpUsers(MockWorkloadDefinition, newUserEvery: 1000, maxUsers: 100, testDurationSecs: 20));

            PerformanceViolationChecker pvc = new PerformanceViolationChecker();

            var perfMetrics = pvc.CalcualteAllMetrics(SendRequests.conCurResponseDict);
        }



        /// <summary>
        /// This is just to test if our mocked webserver is working. Not a PhonenixRunner unit tset. 
        /// </summary>
        [Test, Category("UnitTests")]
        public void Test_MockWebServer_Get()
        {
            // web server must be running. 

            string urlAddress = "http://localhost:8080/test/";
            string responseData = "";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlAddress);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream receiveStream = response.GetResponseStream();
                StreamReader readStream = null;

                if (response.CharacterSet == null)
                {
                    readStream = new StreamReader(receiveStream);
                }
                else
                {
                    readStream = new StreamReader(receiveStream, System.Text.Encoding.GetEncoding(response.CharacterSet));
                }
                responseData = readStream.ReadToEnd();
                response.Close();
                readStream.Close();
            }

            Assert.IsTrue(responseData.Contains("My web page"), $"The words 'My web page' should appear in the response. Expected: 'My web page' ");
        }



    }
}