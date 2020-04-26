using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ADP_DAP_LoadTest
{
  



        /// <summary>
        /// Gets the name of currently executing method.
        /// "Functional" style performance tests can use this to dynamically get the name
        /// of the method being tested. 
        /// </summary>
        public static class MyMethodName
        {
            public static string Show([CallerMemberName] string name = "")
            {
                return name;
            }
        }




        /// <summary>
        /// Really Random Number generator.
        /// This class overcomes limitations in the default .Net random number generator. these limitations
        /// have to do with skewing of the distribution of the random number. This class overcomes these limitations by 
        /// seeding the random number generator with a cryptography key. 
        /// </summary>
        public static class ReallyRandomNumber
        {
            private static readonly RNGCryptoServiceProvider _generator = new RNGCryptoServiceProvider();

            public static int Between(int minimumValue, int maximumValue)
            {
                byte[] randomNumber = new byte[1];

                _generator.GetBytes(randomNumber);

                double asciiValueOfRandomCharacter = Convert.ToDouble(randomNumber[0]);

                // We are using Math.Max, and substracting 0.00000000001, 
                // to ensure "multiplier" will always be between 0.0 and .99999999999
                // Otherwise, it's possible for it to be "1", which causes problems in our rounding.
                double multiplier = Math.Max(0, (asciiValueOfRandomCharacter / 255d) - 0.00000000001d);

                // We need to add one to the range, to allow for the rounding done with Math.Floor
                int range = maximumValue - minimumValue + 1;

                double randomValueInRange = Math.Floor(multiplier * range);

                return (int)(minimumValue + randomValueInRange);
            }
        }









        /// <summary>
        /// A Logging class implementing the Singleton pattern and an internal Queue to be flushed perdiodically.
        /// </summary>
        public class LogWriter
        {
            private static LogWriter instance;
            private static Queue<Log> logQueue;
            private static string logDir = "C:\\log\\";
            private static string logFile = "PhoenixResponseLog_CopyMeBeforeViewing.log";
            private static int maxLogAge = int.Parse("1");
            private static int queueSize = int.Parse("1");
            private static DateTime LastFlushed = DateTime.Now;
            string logPath = logDir + DateTime.Now.Year + "-" +
             DateTime.Now.Month + "-" +
             DateTime.Now.Day + "-" +
             DateTime.Now.Hour + "_" +
             DateTime.Now.Minute + "_" +
             logFile; //

            /// <summary>
            /// Private constructor to prevent instance creation
            /// </summary>
            private LogWriter() { }

            /// <summary>
            /// An LogWriter instance that exposes a single instance
            /// </summary>
            public static LogWriter Instance
            {
                get {
                    // If the instance is null then create one and init the Queue
                    if (instance == null)
                    {
                        instance = new LogWriter();
                        logQueue = new Queue<Log>();
                    }
                    return instance;
                }
            }

            /// <summary>
            /// The single instance method that writes to the log file
            /// </summary>
            /// <param name="message">The message to write to the log</param>
            public void WriteToLog(string message)
            {

                // set up the path and filename
                if (!Directory.Exists(logDir)) Directory.CreateDirectory(logDir);

                // Lock the queue while writing to prevent contention for the log file
                lock (logQueue)
                {
                    // Create the entry and push to the Queue
                    Log logEntry = new Log(message);
                    logQueue.Enqueue(logEntry);

                    // If we have reached the Queue Size then flush the Queue
                    if (logQueue.Count >= queueSize || DoPeriodicFlush())
                    {
                        FlushLog();
                    }
                }
            }

            private bool DoPeriodicFlush()
            {
                TimeSpan logAge = DateTime.Now - LastFlushed;
                if (logAge.TotalSeconds >= maxLogAge)
                {
                    LastFlushed = DateTime.Now;
                    return true;
                }
                else
                {
                    return false;
                }
            }

            /// <summary>
            /// Flushes the Queue to the physical log file
            /// </summary>
            private void FlushLog()
            {
                while (logQueue.Count > 0)
                {
                    Log entry = logQueue.Dequeue();


                    // This could be optimised to prevent opening and closing the file for each write
                    using (FileStream fs = File.Open(logPath, FileMode.Append, FileAccess.Write))
                    {
                        using (StreamWriter log = new StreamWriter(fs))
                        {
                            log.WriteLine(string.Format("{0} {1}", entry.LogTime, entry.Message));
                        }
                    }
                }
            }
        }







        /// <summary>
        /// A Log class to store the message and the Date and Time the log entry was created
        /// </summary>
        public class Log
        {
            public string Message { get; set; }
            public string LogTime { get; set; }
            public string LogDate { get; set; }

            public Log(string message)
            {
                Message = message;
                LogDate = DateTime.Now.ToString("yyyy-MM-dd");
                LogTime = DateTime.Now.ToString("hh:mm:ss.fff tt") + ":";
            }
        }





        /// <summary>
        /// Used to collect response times for any method.
        /// Can be used in multithreaded scenarios. 
        /// How to call: double responseTime =  WithTimer.MeasureResponsetime(<name of test method>);
        /// How to call asynchronously: 
        /// <param name="action">An Action delegate to the method under test.</param>
        /// </summary>
        public class WithTimer : IDisposable
        {
            private readonly Stopwatch _stopwatch;
            private readonly Action<Stopwatch> _action;

            public Stopwatch Watch
            {
                get { return _stopwatch; }
            }
            public double milliseconds
            {
                get { return _stopwatch.ElapsedMilliseconds; }
            }

            public WithTimer(Action<Stopwatch> action = null)
            {
                _action = action ?? (s => Debug.WriteLine(s.ElapsedMilliseconds));
                _stopwatch = new Stopwatch();
                _stopwatch.Start();
            }
            public void Dispose()
            {
                _stopwatch.Stop();
                _action(_stopwatch);
            }

        }




        /// <summary>
        /// Used to collect response times for any method.
        /// Can be used in multithreaded scenarios. 
        /// How to call: double responseTime =  WithTimer.MeasureResponsetime(<name of test method>);
        /// How to call asynchronously: double responseTime = WithSimpleTimer.MeasureResponsetime(new Action(() => testMethodName.Invoke()));
        /// This try catch strategy is adding about 30 ms to each request, and does not correspond to Fiddler.
        /// Use this for static methods only. 
        /// <param name="action">An Action delegate to the method under test.</param>
        /// </summary>
        public class WithSimpleTimer
        {
            public long MeasureResponsetime(Action action)
            {
                Stopwatch sw = Stopwatch.StartNew();

                long TTLB = -99;

                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    string message = ex.Message;
                }
                finally
                {
                    sw.Stop();
                    TTLB = sw.ElapsedMilliseconds;
                }
                return TTLB;
            }
        }




        /// <summary>
        /// Deprecated: This is another way to create a stopwatch and make sure it is disposed.
        /// I think using a function is better. 
        /// </summary>
        public class DisposableStopwatch : IDisposable
        {
            private readonly Stopwatch sw;
            private readonly Action<TimeSpan> f;

            public double milliseconds
            {
                get { return sw.ElapsedMilliseconds; }
            }

            public DisposableStopwatch(Action<TimeSpan> f)
            {
                this.f = f;
                sw = Stopwatch.StartNew();
            }
            public void Dispose()
            {
                sw.Stop();
                f(sw.Elapsed);
            }
        }



        /// <summary>
        /// http://asherman.io/projects/csharp-async-reentrant-locks.html
        /// This might come in handy for some applications. 
        /// </summary>
        class REAsyncLock
        {
            private AsyncLocal<System.Threading.SemaphoreSlim> currentSemaphore =
                new AsyncLocal<SemaphoreSlim>() { Value = new SemaphoreSlim(1) };

            public async Task DoWithLock(Func<Task> body)
            {
                SemaphoreSlim currentSem = currentSemaphore.Value;
                await currentSem.WaitAsync();
                var nextSem = new SemaphoreSlim(1);
                currentSemaphore.Value = nextSem;
                try
                {
                    await body();
                }
                finally
                {
                    Debug.Assert(nextSem == currentSemaphore.Value);
                    await nextSem.WaitAsync();
                    currentSemaphore.Value = currentSem;
                    currentSem.Release();
                }
            }

        }





    }

