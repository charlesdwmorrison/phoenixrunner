using RestSharp;
using System;
using System.Collections.Generic;

namespace ADP_DAP_LoadTest
{
    public class Req
    {
        public string uri;
        public string body;
        public Method method;
        public DateTime reqStartTime;

        public Dictionary<string, string> extractText;
        public string correlationVariableName;
        public string correlatedValue;
        public bool useExtractText = false;

        public string leftBoundary; // the correlation boundary for the target in the *response*
        public string rightBoundary;
       
    }
}
