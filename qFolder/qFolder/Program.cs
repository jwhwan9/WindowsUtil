using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;

namespace qFolder
{
    class Program
    {
        static void Main(string[] args)
        {
            
            string myDC = Environment.GetEnvironmentVariable("LOGONSERVER");
            checkFiles("\\"+myDC+@"\" + "netlogon");
            Console.ReadKey();
        }

        static void checkFolder(string Folder)
        {
            try
            {
                string dirPath = Folder;

                List<string> dirs = new List<string>(Directory.EnumerateDirectories(dirPath));
                dirs.Insert(0, dirPath);

                foreach (var dir in dirs)
                {
                    //Console.WriteLine("{0}", dir.Substring(dir.LastIndexOf("\\") + 1));

                    FileInfo myFileInfo = new FileInfo(dir);
                    Console.WriteLine("{0}", dir);

                    FileSecurity fileSec = myFileInfo.GetAccessControl();
                    var authRuleColl =
                           fileSec.GetAccessRules(true, true, typeof(NTAccount));

                    foreach (FileSystemAccessRule fsaRule in authRuleColl)
                    {
                        Console.WriteLine("\t IdentityReference: {0}",
                            fsaRule.IdentityReference);
                        Console.WriteLine("\t AccessControlType: {0}",
                            fsaRule.AccessControlType);
                        Console.WriteLine("\t FileSystemRights: {0}",
                            fsaRule.FileSystemRights);
                        Console.WriteLine();
                    }

                    //Console.ReadKey();
                }
                Console.WriteLine("{0} directories found.", dirs.Count);
            }
            catch (UnauthorizedAccessException UAEx)
            {
                Console.WriteLine(UAEx.Message);
            }
            catch (PathTooLongException PathEx)
            {
                Console.WriteLine(PathEx.Message);
            }
        }

        static void checkFiles(string Folder)
        {
            try
            {
                var files = from file in Directory.EnumerateFiles(Folder, "*", SearchOption.AllDirectories)
                            //from line in File.ReadLines(file)
                            //where line.Contains("Microsoft")
                            select new
                            {
                                File = file
                                //,Line = line
                            };

                foreach (var f in files)
                {
                    //Console.WriteLine("{0}\t{1}", f.File, f.Line);                    
                    FileInfo myFileInfo = new FileInfo(f.File);
                    Console.WriteLine("{0}", f.File);

                    FileSecurity fileSec = myFileInfo.GetAccessControl();
                    var authRuleColl =
                           fileSec.GetAccessRules(true, true, typeof(NTAccount));

                    foreach (FileSystemAccessRule fsaRule in authRuleColl)
                    {
                        Console.WriteLine("\t IdentityReference: {0}",
                            fsaRule.IdentityReference);
                        Console.WriteLine("\t AccessControlType: {0}",
                            fsaRule.AccessControlType);
                        Console.WriteLine("\t FileSystemRights: {0}",
                            fsaRule.FileSystemRights);
                        Console.WriteLine();
                    }

                }
                Console.WriteLine("{0} files found.", files.Count().ToString());
            }
            catch (UnauthorizedAccessException UAEx)
            {
                //Console.WriteLine(UAEx.Message);
            }
            catch (PathTooLongException PathEx)
            {
                //Console.WriteLine(PathEx.Message);
            }
        }
    }
}
