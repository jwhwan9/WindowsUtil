﻿using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace MultiPortScan
{
    class PortScanner
    {        
        private string host;
        private PortList portList;
        private bool turnOff = true;
        private int count = 0;
        public int tcpTimeout ;

        private class isTcpPortOpen
        {
            public TcpClient MainClient { get; set; }
            public bool tcpOpen { get; set; }
        }


        public PortScanner(string host, int portStart, int portStop , int timeout)
        {
            this.host = host;
            portList = new PortList(portStart, portStop);
            tcpTimeout = timeout;
           
        }

        public void addPorts(int[] PortsArray){
            portList.addPorts(PortsArray);
        }

        public void start(int threadCounter)
        {
            Thread[] myThreads = new Thread[threadCounter];

            #region Create multiple thread to perform PortScan
            for (int i = 0; i < threadCounter; i++)
            {
                var handle = new EventWaitHandle(false, EventResetMode.ManualReset);
                Thread thread1 = new Thread(new ThreadStart(RunScanTcp));
                myThreads[i] = thread1;
                thread1.Start();
            }
            #endregion

            #region Waitfor All Thread finished
            for (int i = 0; i < threadCounter; i++)
            {
                myThreads[i].Join();
            }
            #endregion

        }

        public void RunScanTcp()
        {
          
            int port;

            //while there are more ports to scan 
            while ((port = portList.NextPort()) != -1)
            {
                count = port;

                Thread.Sleep(100); //lets be a good citizen to the cpu
                
                Console.Title = host+" Port Count : " + count.ToString();
                //Console.WriteLine("try {0},TCP,{1} ", host, port);

                try
                {
                    
                        string myHost = host;
                        Connect(myHost, port, tcpTimeout);
                        Console.WriteLine("{0},TCP,{1},probed ", myHost, port);
                    
                }
                catch
                {        
                    continue;
                }
              
                /*
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine();
                Console.WriteLine("{0} TCP  {1} is open ", host, port);
                 */ 
                try
                {
                    /*
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    //grabs the banner / header info etc..
                    Console.WriteLine(BannerGrab(host, port, tcpTimeout)); 
                   */
                   
                }
                catch (Exception ex)
                {
                    /*
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Could not retrieve the Banner ::Original Error = " + ex.Message);
                    Console.ResetColor();
                     */ 
                }
                /*
                Console.ForegroundColor = ConsoleColor.Green;
                string webpageTitle = GetPageTitle("http://" + host + ":" + port.ToString());

                if(!string.IsNullOrWhiteSpace(webpageTitle))
                {
                    //this gets the html title of the webpage
                    Console.WriteLine("Webpage Title = " + webpageTitle + "Found @ :: " + "http://" + host + ":" + port.ToString()); 
                  
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.DarkMagenta;
                    Console.WriteLine("Maybe A Login popup or a Service Login Found @ :: " + host + ":" + port.ToString());
                    Console.ResetColor();
                }
               */

                Console.ResetColor();

            }
            
              
                if (turnOff == true )
                {

                    turnOff = false;
                    /*
                    Console.WriteLine();
                    Console.WriteLine("Scan Complete !!!");

                    Console.ReadKey();
                    */
                }

        }
    //method for returning tcp client connected or not connected
        public TcpClient Connect(string hostName, int port, int timeout)
        {
            var newClient = new TcpClient();

            var state = new isTcpPortOpen
            {
                MainClient = newClient, tcpOpen = true
            };

            IAsyncResult ar = newClient.BeginConnect(hostName, port, AsyncCallback, state);
            state.tcpOpen = ar.AsyncWaitHandle.WaitOne(timeout, false);

            if (state.tcpOpen == false || newClient.Connected == false)
            {
                throw new Exception();

            }
            return newClient;
        }

        //method for Grabbing a webpage banner / header information
        public string BannerGrab(string hostName, int port, int timeout)
        {
            var newClient = new TcpClient(hostName ,port);

           
            newClient.SendTimeout = timeout;
            newClient.ReceiveTimeout = timeout;
            NetworkStream ns = newClient.GetStream();
            StreamWriter sw = new StreamWriter(ns);

            //sw.Write("GET / HTTP/1.1\r\n\r\n");

            sw.Write("HEAD / HTTP/1.1\r\n\r\n"
                + "Connection: Closernrn");

            sw.Flush();

            byte[] bytes = new byte[2048];
            int bytesRead = ns.Read(bytes, 0, bytes.Length);
            string response = Encoding.ASCII.GetString(bytes, 0, bytesRead);

            return response;
        }


        //async callback for tcp clients
        void AsyncCallback(IAsyncResult asyncResult)
        {
            var state = (isTcpPortOpen)asyncResult.AsyncState;
            TcpClient client = state.MainClient;

            try
            {
                client.EndConnect(asyncResult);
            }
            catch
            {
                return;
            }

            if (client.Connected && state.tcpOpen)
            {
                return;
            }
               
            client.Close();
        }

        static string GetPageTitle(string link)
        {
            try
            {

                WebClient x = new WebClient();
                string sourcedata = x.DownloadString(link);
                string getValueTitle = Regex.Match(sourcedata, @"\<title\b[^>]*\>\s*(?<Title>[\s\S]*?)\</title\>", RegexOptions.IgnoreCase).Groups["Title"].Value;
              
                return getValueTitle;
              
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Could not connect. Error:" + ex.Message);
                Console.ResetColor();
                return "";
            }

           
        }

    }
}
