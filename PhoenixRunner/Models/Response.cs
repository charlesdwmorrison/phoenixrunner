﻿using System;
using System.Collections.Generic;
using System.Text;

namespace ADP_DAP_LoadTest
{
    public class Response
    {


        public string reqUri = "";
        public string reqVerb = ""; // GET, POST, PUT, etc. 
        public string reqBody = "";
        public string reqUriParams = ""; // This would be for REST calls. 
        public string reqDescription ="";  // E.g., landing/takeoff
        
        public int responseId;
        public bool exceptionThrown;
        public string responseStatus = "{Response: 'Not Yet Received.'}";
        public string responseStatsCode = "-99"; // 500, 200 404, etc. -99 indicates not yet received.  This text is included in all HTTP responses, like OK=200; 
        public long responseTtlb = 0;
        public DateTime responseTimeReceived = new DateTime(1972, 1, 1, 0, 0, 0); //  We will use this to calculate throughput. 
        public bool responseExceptionThrown = false;
        public string responseExceptionMessage = "";
        public string responseBody = "";
        public DateTime requestTimeSent = new DateTime(1972, 1, 1, 0, 0, 0); //  We will use this to calculate throughput. 

    }
}
