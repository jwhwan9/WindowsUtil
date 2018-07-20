using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Threading.Tasks;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Diagnostics;

namespace qDomain
{
    class Program
    {
        static void Main(string[] args)
        {
            string DomainName = "";
            string Keyword = "";

            DomainName = System.Environment.GetEnvironmentVariable("USERDOMAIN");

            if (args.Length == 0)
            {
                Console.WriteLine("qDomain.exe <Domain> <Group Keyword>");
            }else if (args.Length == 1)
            {
                DomainName = args[0];
            }
            else if (args.Length == 2)
            {
                DomainName = args[0];
                Keyword = args[1];
            }

            ListGroup(DomainName, Keyword);

            ListDCInfo(DomainName);
            
            //Console.ReadLine();
        }

        static void ListDCInfo(string DomainName)
        {
            string WinDir = System.Environment.GetEnvironmentVariable("WINDIR");
            string UserName = System.Environment.GetEnvironmentVariable("UserName");
            string LogonServer = System.Environment.GetEnvironmentVariable("LogonServer");

            // C:\Windows\System32 -> C:\Windows\Sysnative
            Console.WriteLine(GetCmdOutput(WinDir + "\\Sysnative\\nltest.exe", "/dclist:" + DomainName));
            Console.WriteLine(GetCmdOutput(WinDir + "\\Sysnative\\nltest.exe", "/whowill:" + DomainName + " " + UserName));
            Console.WriteLine(GetCmdOutput(WinDir + "\\Sysnative\\nltest.exe", "/server:" + LogonServer + " " + "/sc_query:" + DomainName));

        }

        static string GetCmdOutput(string CommandExe, string CommandArg)
        {
            string line = "";
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = CommandExe,
                    Arguments = CommandArg,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };

            proc.Start();
            Console.WriteLine("cmd:> "+CommandExe+" "+CommandArg);
            while (!proc.StandardOutput.EndOfStream)
            {
                 line+=proc.StandardOutput.ReadLine()+System.Environment.NewLine;
                // do something with line
            }

            return line;
        }

        static void ListGroup(string DomainName, string Keyword)
        {
            // create your domain context
            PrincipalContext ctx = new PrincipalContext(ContextType.Domain);

            // define a "query-by-example" principal - here, we search for a GroupPrincipal 
            GroupPrincipal qbeGroup = new GroupPrincipal(ctx);

            // create your principal searcher passing in the QBE principal    
            PrincipalSearcher srch = new PrincipalSearcher(qbeGroup);

            Console.WriteLine("DomainName,Group.SamName,Group.Description,user.SamAccountName,user.UserPrincipalName,user.LastLogon,user.LastBadPasswordAttempt,user.Name,user.Enabled,user.IsAccountLockedOut,user.LastPasswordSet,user.SID");

            // find all matches
            foreach (var found in srch.FindAll())
            {
                if (Keyword.Trim() == "" || found.Name.Contains(Keyword))
                {
                    //Console.WriteLine(found+"@"+DomainName);
                    //Console.WriteLine("=====");
                    
                    using (var context = new PrincipalContext(ContextType.Domain, DomainName))
                    {
                        using (var group = GroupPrincipal.FindByIdentity(context, found.Name))
                        {
                            if (group == null)
                            {
                                //MessageBox.Show("Group does not exist");
                            }
                            else
                            {
                                var users = group.GetMembers(true);
                                
                                try
                                {
                                    foreach (UserPrincipal user in users)
                                    {
                                        
                                        //user variable has the details about the user 
                                        Console.WriteLine("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11}", 
                                            DomainName,                                                                                        
                                            found.Name, 
                                            found.Description,
                                            user.SamAccountName, 
                                            user.UserPrincipalName, 
                                            user.LastLogon, 
                                            user.LastBadPasswordAttempt, 
                                            user.Name, 
                                            user.Enabled, 
                                            user.IsAccountLockedOut(), 
                                            user.LastPasswordSet,
                                            user.Sid);
                                    }
                                }
                                catch (Exception ex) { }
                            }
                        }
                    }
                }
                // do whatever here - "found" is of type "Principal" - it could be user, group, computer.....          
            }
        }        
    }
}
