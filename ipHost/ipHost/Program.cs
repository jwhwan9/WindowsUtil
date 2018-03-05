using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace ipHost
{
    class Program
    {
        static void Main(string[] args)
        {
            string host = "";
                        
            if (args.Length == 1)
            {
                host = args[0];
                GetHostIP(host);
            }
            else if (args.Length == 2 && args[0] == "-f")
            {
                System.IO.StreamReader file = new System.IO.StreamReader(args[1]);
                string line = "";
                while ((line = file.ReadLine()) != null)
                {
                    GetHostIP(line.Trim());
                }

                file.Close();  

            }
            else
            {
                Console.WriteLine("Usage:");
                Console.WriteLine();
                Console.WriteLine("ipHost myServerHostName");
                Console.WriteLine("ipHost -f HostFile.txt");

                return;
            }
                                                
        }

        static string GetHostIP(string host)
        {
            IPHostEntry hostEntry;
            string HostIP = "";
            try
            {
                hostEntry = Dns.GetHostEntry(host);
                HostIP = hostEntry.HostName + "," + hostEntry.AddressList[0].ToString();
            }
            catch (Exception ex)
            {
                HostIP = host + "," + "0,0,0,0";
            }
            Console.WriteLine(HostIP);
            return HostIP;
        }


    }
}
