using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Management;

namespace qProcess
{
    class Program
    {
        static void Main(string[] args)
        {            
            Process[] processlist = Process.GetProcesses();
            Console.WriteLine("Process,PID,Title,Owner,SessionID,ServiceName,Svchost Path,Commandline");

            foreach (Process theprocess in processlist)
            {                
                //theprocess.                                
                Console.WriteLine("{0},{1},{2},{3},{4},{5},{6}", 
                    theprocess.ProcessName, 
                    theprocess.Id,
                    theprocess.MainWindowTitle,
                    GetProcessOwner(theprocess),
                    theprocess.SessionId,
                    GetServiceName(theprocess), // [3] 如果是 Service(svchost)，會自動產生三個欄位: DisplayName,Account Owner,PathName
                    GetCommandLine(theprocess));                               
            }
                        
            //Console.ReadKey();
        }

        public static string GetServiceName(Process process)
        {
            string servicePath = ",";

            using (ManagementObjectSearcher Searcher = new ManagementObjectSearcher(
            "SELECT * FROM Win32_Service WHERE ProcessId =" + "\"" + process.Id + "\""))
            {
                foreach (ManagementObject service in Searcher.Get())
                    servicePath = service["DisplayName"].ToString() + "," + service["SystemName"].ToString() + "," + service["StartName"].ToString() + "," + service["PathName"].ToString();
            }
            return servicePath;
        }

        private static string GetCommandLine(Process process)
        {
            string myProcessCommand = "";

            var q = string.Format("select CommandLine from Win32_Process where ProcessId='{0}'", process.Id);
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(q);
            ManagementObjectCollection result = searcher.Get();
            foreach (ManagementObject obj in result)
                myProcessCommand = (string)obj["CommandLine"];

            return myProcessCommand;
        }

        private static string GetProcessOwner(Process process)
        {
            string query = "Select * From Win32_Process Where ProcessID = " + process.Id;
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
            ManagementObjectCollection processList = searcher.Get();

            foreach (ManagementObject obj in processList)
            {
                string[] argList = new string[] { string.Empty, string.Empty };
                int returnVal = Convert.ToInt32(obj.InvokeMethod("GetOwner", argList));
                if (returnVal == 0)
                {
                    // return DOMAIN\user
                    return argList[1] + "\\" + argList[0];
                }
            }

            return "N/A";
        }
    }
}
