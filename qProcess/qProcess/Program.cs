using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Management;

namespace qProcess
{
    class Program
    {
        static void Main(string[] args)
        {
            
            Process[] processlist = Process.GetProcesses();

            Console.WriteLine("Process,PID,Commandline");

            foreach (Process theprocess in processlist)
            {                
                //theprocess.                
                Console.WriteLine("{0},{1},{2},{3},{4}", theprocess.ProcessName, theprocess.Id,theprocess.MainWindowTitle,GetProcessOwner(theprocess), GetCommandLine(theprocess));
                               
            }
                        
            //Console.ReadKey();
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
