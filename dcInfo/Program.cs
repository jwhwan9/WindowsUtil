using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;
using System.DirectoryServices.AccountManagement;

namespace dcInfo
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Logon DC: "+System.Environment.GetEnvironmentVariable("logonserver"));
            Console.WriteLine("Logon ID: " + System.Environment.GetEnvironmentVariable("UserName"));
            string myDomain = ListDomain();
            EnumerateComputer(myDomain);
            EnumerateUser(myDomain);
            EnumerateGroup(myDomain);            
        }

        static string ListDomain()
        {
            string myDomain = "";
            using (var forest = Forest.GetCurrentForest())
            {

                foreach (Domain domain in forest.Domains)
                {
                    myDomain = domain.Name;
                    Console.WriteLine(myDomain);
                    domain.Dispose();
                }
            }
            return myDomain;
        }

        static void EnumerateComputer(string domainName)
        {

            PrincipalContext ctx = new PrincipalContext(ContextType.Domain, domainName);

            ComputerPrincipal cP = new ComputerPrincipal(ctx);

            cP.Name = "*";

            PrincipalSearcher ps = new PrincipalSearcher();

            ps.QueryFilter = cP;

            PrincipalSearchResult<Principal> result = ps.FindAll();

            using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"Computer.csv"))
            {
                file.WriteLine("Name" + "," + "LastLogon" + "," + "BadLogonCount" + "," + "SamAccountName" + "," + "ScriptPath" + "," + "Sid" + "," + "UserPrincipalName");
                foreach (ComputerPrincipal p in result)
                {

                    // do something with p.Name;
                    file.WriteLine(p.Name + "," + p.LastLogon + "," + p.BadLogonCount + "," + p.SamAccountName + "," + p.ScriptPath + "," + p.Sid + "," + p.UserPrincipalName);
                }
            }
        }

        static void EnumerateUser(string domainName)
        {
            PrincipalContext ctx = new PrincipalContext(ContextType.Domain, domainName);

            UserPrincipal cP = new UserPrincipal(ctx);

            cP.Name = "*";

            PrincipalSearcher ps = new PrincipalSearcher();

            ps.QueryFilter = cP;

            PrincipalSearchResult<Principal> result = ps.FindAll();

            using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"User.csv"))
            {
                file.WriteLine("Name" + "," + "LastLogon" + "," + "BadLogonCount" + "," + "SamAccountName" + "," + "Sid" + "," + "UserPrincipalName");
                foreach (UserPrincipal p in result)
                {

                    // do something with p.Name;
                    file.WriteLine(p.Name + "," + p.LastLogon + "," + p.BadLogonCount + "," + p.SamAccountName + "," + p.Sid + "," + p.UserPrincipalName);
                }
            }
        }


        static void EnumerateGroup(string domainName)
        {
            PrincipalContext ctx = new PrincipalContext(ContextType.Domain, domainName);

            GroupPrincipal cP = new GroupPrincipal(ctx);

            cP.Name = "*";

            PrincipalSearcher ps = new PrincipalSearcher();

            ps.QueryFilter = cP;

            PrincipalSearchResult<Principal> result = ps.FindAll();

            using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"Group.csv"))
            {
                file.WriteLine("Name" + "," + "Sid" + "," + "Description" + "," + "UserPrincipalName" + "," + "SamAccountName");
                foreach (GroupPrincipal p in result)
                {

                    // do something with p.Name;
                    file.WriteLine(p.Name + ","+ p.Sid + "," + p.Description+","+p.UserPrincipalName+","+p.SamAccountName);
                }
            }
        }
    }
}
