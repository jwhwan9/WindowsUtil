using System;

namespace MultiPortScan
{
    /// <summary>
    /// A Console type Multi Port TCP Scanner
    /// Author : Philip Murray
    /// </summary>

    class Program
    {

        static void Main(string[] args)
        {
            string host;
            int portStart;
            int portStop;
            int Threads;
            int timeout;
            string ip;
            string startPort;
            string endPort;   

            youGotItWrong: //goto: Start Again

            //this is for the user to select a host ip
            endPort = "1024";
            ip = "127.0.0.1";
            startPort = "0";
            Threads = 15;
            timeout = 1*1000;

            if (args.Length >= 3) { 
                ip = args[0];
                startPort = args[1];
                endPort = args[2];

                if (args.Length == 4)
                {
                    int numThread;
                    if (int.TryParse(args[3], out numThread))
                    {
                        if (numThread <= 0)
                        {
                            Threads = 1;
                        }else if(numThread>50){
                            Threads = 50;
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine("QPortScan.exe [ip] [startPort] [endPort] [threads]");
            }
            
            
            host = ip;

            //this is for the user to select the start port            
                       

            //THIS CHECKS TO SEE IF IT THE START PORT CAN BE PARSED OUT
            int number;
            bool resultStart = int.TryParse(startPort, out number);

            if (resultStart)
            {
                portStart = int.Parse(startPort);
            }

            else
            {
                Console.WriteLine("Try Again NOOOB!!");
                goto youGotItWrong;
               // return;
            }
                                             
            //THIS CHECKS TO SEE IF IT THE END PORT CAN BE PARSED OUT
            int number2;
            bool resultEnd = int.TryParse(endPort, out number2);

            if (resultEnd)
            {
                portStop = int.Parse(endPort);
            }

            else
            {
                Console.WriteLine("Try Again NOOOB!!");

                goto youGotItWrong;
               // return;
            }

            try
            {

                host = ip;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }

            if (resultStart == true && resultEnd == true)
            {
                try
                {

                    portStart = int.Parse(startPort);
                    portStop = int.Parse(endPort);

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return;
                }

            }

            PortScanner ps = new PortScanner(host, portStart, portStop , timeout);
            ps.start(Threads);
            
        }
        
    }
}
