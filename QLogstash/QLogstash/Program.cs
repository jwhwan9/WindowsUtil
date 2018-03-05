using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace QLogstash
{
    class Program
    {
        static void Main(string[] args)
        {
            string[] myHost = { "172.16.160.57", "172.16.160.58", "172.16.160.59" };
            try { 
            myHost = File.ReadLines("QLogstash.txt").ToArray();
            }
            catch (Exception ex)
            {
                Console.WriteLine("File Not found: QLogstash.txt");
            }
            loopConsoleOutput(myHost);
            Console.ReadKey();

        }

        static void loopConsoleOutput(string[] myHost)
        {
            for (int i = 0; i < myHost.Length; i++)
            {
                string myConnect = "ACK";
                string myInfo = myHost[i];

                try
                {
                    dynamic myLogs = getFormatedInfo(myHost[i], "_node");
                    myInfo = myHost[i] + " " + myLogs.jvm.pid;
                    
                    myLogs = getFormatedInfo(myHost[i], "_node/stats");
                    float myInputDuration = (float)(myLogs.pipeline.events.queue_push_duration_in_millis) / (Int64)myLogs.pipeline.events["in"];
                    float myTotalDuration = (float)(myLogs.pipeline.events.duration_in_millis) / (Int64)myLogs.pipeline.events["out"];

                    myInfo = myInfo
                        + " " + myLogs.process.cpu.percent + "%"
                        + " " + myLogs.process.cpu.load_average["5m"] + "%"
                        + " " + SizeSuffix((Int64)(myLogs.process.mem.total_virtual_in_bytes))
                        + " " + myLogs.pipeline.events["in"]+".i"
                        + " " + myInputDuration.ToString("n2")+"ms" //MilliSecHuman((Int64)(myLogs.pipeline.events.duration_in_millis))
                        + " " + myLogs.pipeline.events["filtered"] + ".f"
                        + " " + myLogs.pipeline.events["out"] + ".o"
                        + " " + myTotalDuration.ToString("n2") + "ms" //MilliSecHuman((Int64)(myLogs.pipeline.events.queue_push_duration_in_millis))
                        ;
                }
                catch (Exception ex)
                {
                    if (myInfo.Equals(myHost[i]))
                    {
                        myConnect = "NAK";
                    }
                    else
                    {
                        myConnect = "ERR";
                    }
                }

                Console.WriteLine(myConnect + " " + myInfo);
            }

        }

        static JObject getFormatedInfo(string Info_Host, string Info_Type)
        {
            JObject myJSON;
            myJSON = JObject.Parse(getLogstashInfo(Info_Host, Info_Type, 9600));

            return myJSON;
        }

        static string getLogstashInfo(string Url_Host, string Url_Parameter, int Port = 9600)
        {
            string url = "http://" + Url_Host + ":" + Port + @"/" + Url_Parameter;

            var request = WebRequest.Create(url);
            request.Timeout = 3000;
            string text = "";

            try
            {
                var response = (HttpWebResponse)request.GetResponse();

                using (var sr = new StreamReader(response.GetResponseStream()))
                {
                    text = sr.ReadToEnd();
                }
            }
            catch (Exception ex) { }

            return text;
        }

        static string MilliSecHuman(Int64 value)
        {
            TimeSpan t = TimeSpan.FromMilliseconds(value);
            string answer = string.Format("{0:D2}:{1:D2}m:{2:D2}:{3:D3}ms",
                                    t.Hours,
                                    t.Minutes,
                                    t.Seconds,
                                    t.Milliseconds);
            return answer;
        }

        static readonly string[] SizeSuffixes = { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
        static string SizeSuffix(Int64 value, int decimalPlaces = 1)
        {
            if (decimalPlaces < 0) { throw new ArgumentOutOfRangeException("decimalPlaces"); }
            if (value < 0) { return "-" + SizeSuffix(-value); }
            if (value == 0) { return string.Format("{0:n" + decimalPlaces + "} bytes", 0); }

            // mag is 0 for bytes, 1 for KB, 2, for MB, etc.
            int mag = (int)Math.Log(value, 1024);

            // 1L << (mag * 10) == 2 ^ (10 * mag) 
            // [i.e. the number of bytes in the unit corresponding to mag]
            decimal adjustedSize = (decimal)value / (1L << (mag * 10));

            // make adjustment when the value is large enough that
            // it would round up to 1000 or more
            if (Math.Round(adjustedSize, decimalPlaces) >= 1000)
            {
                mag += 1;
                adjustedSize /= 1024;
            }

            return string.Format("{0:n" + decimalPlaces + "}{1}",
                adjustedSize,
                SizeSuffixes[mag]);
        }
    }
}
