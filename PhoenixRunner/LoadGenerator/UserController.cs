using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace ADP_DAP_LoadTest
{
    public class UserController
    {
        readonly LogWriter writer = LogWriter.Instance;

        private static int numThreads = 0; // number of Threads. This is incremented by throttler.release().


        /// <summary>
        /// Adds another user (thread) to process a new instance of the workload.
        /// </summary>
        /// <param name="newUserEvery">Time in milliseconds of the wait time before adding another thread. E.g., every three seconds.</param>
        /// <returns>A Task</returns>
        public async Task RampUpUsers(Action act=null, int newUserEvery=2000, int maxUsers=2, long testDurationSecs=360 )
        {
            var tasksInProgress = new List<Task>();
            Stopwatch sw = new Stopwatch();
            sw.Start();
            while ((sw.ElapsedMilliseconds < testDurationSecs * 1000) & (numThreads < maxUsers)) // loop as long as load test lasts. 
            {
                Thread.Sleep(newUserEvery);
                Interlocked.Increment(ref numThreads);
                
                var t = Task.Run(() => act());
                tasksInProgress.Add(t);
            }
            await Task.WhenAll(tasksInProgress);
        }


    }
}
