using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics.Eventing.Reader;

namespace QWinEvent
{
    class Program
    {
        #region Common Variable
        public static Dictionary<int, int> filteredEventID;
        public static Dictionary<string, string> filteredEventCondition;
        public static string EventLogFile = @"Security.evtx";
        public static string[] IREventHeader;

        enum IREvent
        {
            DateTime, MachineName, RecordID, LogName, EventID, Task, TaskID, Provider, Brief,
            SubjectLogonId, SubjectUserSid, SubjectUserName,SubjectDomainName, SubjectGUID,
            TargetLogonId, TargetUserSid, TargetUserName, TargetDomainName,
            LogonType, LogonTypeName, ProcessID, ProcessName,
            SourceHost, SourceIP, SourcePort,
            TargetHost, TargetIP, TargetPort,
            LogonProcess, AuthPackage,
            DCNtlmAuth, DCNtlmErrorCode, DCNtlmError,
            KerberosTktOption,KerberosErrorCode,KerberosEncryptionType,KerberosPreAuthenticationType,
            Privilege
        };
        #endregion

        #region IREvent Schema
        static void NewIREventString(bool isConsoleOutput)
        {
            var myEnumMemberCount = Enum.GetNames(typeof(IREvent)).Length;
            IREventHeader = new string[myEnumMemberCount];

            #region Common Event Info
            IREventHeader[(int)IREvent.DateTime] = "DateTime";
            IREventHeader[(int)IREvent.MachineName] = "MachineName";
            IREventHeader[(int)IREvent.RecordID] = "RecordID";
            IREventHeader[(int)IREvent.LogName] = "LogName";
            IREventHeader[(int)IREvent.EventID] = "EventID";
            IREventHeader[(int)IREvent.Task] = "Task";
            IREventHeader[(int)IREvent.TaskID] = "TaskID";
            IREventHeader[(int)IREvent.Provider] = "Provider";
            IREventHeader[(int)IREvent.Brief] = "Brief";
            #endregion

            #region Subject Info 4624
            IREventHeader[(int)IREvent.SubjectLogonId] = "SubjectLogonId";
            IREventHeader[(int)IREvent.SubjectUserSid] = "SubjectUserSid";
            IREventHeader[(int)IREvent.SubjectUserName] = "SubjectUserName";
            IREventHeader[(int)IREvent.SubjectDomainName] = "SubjectDomainName";
            IREventHeader[(int)IREvent.SubjectGUID] = "SubjectGUID";
            #endregion

            #region Target Info 4624
            IREventHeader[(int)IREvent.TargetLogonId] = "TargetLogonId";
            IREventHeader[(int)IREvent.TargetUserSid] = "TargetUserSid";
            IREventHeader[(int)IREvent.TargetUserName] = "TargetUserName";
            IREventHeader[(int)IREvent.TargetDomainName] = "TargetDomainName";
            #endregion

            #region Process Info 4624
            IREventHeader[(int)IREvent.LogonType] = "LogonType";
            IREventHeader[(int)IREvent.LogonTypeName] = "LogonTypeName";
            IREventHeader[(int)IREvent.ProcessID] = "ProcessID";
            IREventHeader[(int)IREvent.ProcessName] = "ProcessName";            
            #endregion      
      
            #region Network Info 4624
            IREventHeader[(int)IREvent.SourceHost] = "SourceHost";
            IREventHeader[(int)IREvent.SourceIP] = "SourceIP";
            IREventHeader[(int)IREvent.SourcePort] = "SourcePort";

            IREventHeader[(int)IREvent.TargetHost] = "TargetHost";
            IREventHeader[(int)IREvent.TargetIP] = "TargetIP";
            IREventHeader[(int)IREvent.TargetPort] = "TargetPort";     
            #endregion

            #region AuthInfo 4625
            IREventHeader[(int)IREvent.LogonProcess] = "LogonProcess";
            IREventHeader[(int)IREvent.AuthPackage] = "AuthPackage";            
            #endregion

            #region DCNtlm 4776
            IREventHeader[(int)IREvent.DCNtlmAuth] = "DCNtlmAuth";
            IREventHeader[(int)IREvent.DCNtlmErrorCode] = "DCNtlmErrorCode";
            IREventHeader[(int)IREvent.DCNtlmError] = "DCNtlmError";
            #endregion

            #region DC Kerberos 4768
            IREventHeader[(int)IREvent.KerberosTktOption] = "KerberosTktOption";
            IREventHeader[(int)IREvent.KerberosErrorCode] = "KerberosErrorCode";
            IREventHeader[(int)IREvent.KerberosEncryptionType] = "KerberosEncryptionType";
            IREventHeader[(int)IREvent.KerberosPreAuthenticationType] = "KerberosPreAuthenticationType";
            #endregion

            #region Privilege 4672
            IREventHeader[(int)IREvent.Privilege] = "Privilege";            
            #endregion

            if (isConsoleOutput)
            {
                dumpStrings2CSV(IREventHeader);                
            }
        }
        #endregion

        #region Strings Arrary to CSV (string column with ",")
        static void dumpStrings2CSV(string[] pStrings)
        {
            for (int i = 0; i < pStrings.Length; i++)
            {
                Console.Write(pStrings[i]);
                if (i < (pStrings.Length - 1) ) { 
                    Console.Write(",");
                }
            }
            Console.WriteLine();
        }
        #endregion

        static void Main(string[] args)
        {
            

            if (args.Length >= 1)
            {
                EventLogFile = args[0];
                Console.WriteLine(EventLogFile);
            }

            if (!File.Exists(EventLogFile))
            {
                Console.WriteLine("usage:");
                Console.WriteLine("QWinEvent.exe <Security.evtx>");
                Environment.Exit(0);
            }
            
            NewIREventString(true);

            filteredEventID = new Dictionary<int, int>();
            filteredEventCondition = new Dictionary<string, string>();         
  
            filteredEventID = new System.Collections.Generic.Dictionary<int,int>();
            
            // Enriched Event                         
            filteredEventID.Add(4624, 4624);
            filteredEventID.Add(4672, 4672);
            filteredEventID.Add(4662, 4662);
            filteredEventID.Add(4634, 4634);
            filteredEventID.Add(4648, 4648);
            filteredEventID.Add(4776, 4776);            
            filteredEventID.Add(4768, 4768);            
            filteredEventID.Add(4769, 4769);
            filteredEventID.Add(4625, 4625);            

            /// Not Enrich yet            
            filteredEventID.Add(7034, 7034);
            filteredEventID.Add(7035, 7035);
            filteredEventID.Add(7036, 7036);
            filteredEventID.Add(7040, 7040);
            filteredEventID.Add(4778, 4778);                        

            filteredEventCondition = new System.Collections.Generic.Dictionary<string, string>();
            /*
            filteredEventCondition.Add("S-1-5-17", "S-1-5-17");
            filteredEventCondition.Add("S-1-5-18", "S-1-5-18");
            filteredEventCondition.Add("S-1-5-19", "S-1-5-19");
            filteredEventCondition.Add("S-1-5-20", "S-1-5-20");
            */
            using (var reader = new EventLogReader(EventLogFile, PathType.FilePath))
            {                
                EventRecord record;
                int i = 0;
                while ((record = reader.ReadEvent()) != null)
                {
                    
                    using (record)
                    {
                        if (!filteredEventID.ContainsKey(record.Id)) continue;

                        //if (i++ > 0) break;

                        var meta = new ProviderMetadata(record.ProviderName).Events.Where(evt => evt.Id == record.Id).FirstOrDefault();
                        IList<EventProperty> Properties = record.Properties;

                        if (!isFilterOut(record)) { 
                        
                            string[] IREventLog = new string[Enum.GetNames(typeof(IREvent)).Length];   
                         
                            #region Common Event Info
                            IREventLog[(int)IREvent.DateTime] = record.TimeCreated.Value.ToString("u");
                            IREventLog[(int)IREvent.MachineName] = record.MachineName;
                            IREventLog[(int)IREvent.RecordID] = record.RecordId.ToString();
                            IREventLog[(int)IREvent.LogName] = record.LogName;
                            IREventLog[(int)IREvent.EventID] = record.Id.ToString();
                            IREventLog[(int)IREvent.Task] = record.TaskDisplayName;
                            IREventLog[(int)IREvent.TaskID] = record.Task.ToString();
                            IREventLog[(int)IREvent.Provider] = record.ProviderName.ToString();
                            IREventLog[(int)IREvent.Brief] = record.FormatDescription().Split(new[] { '\r', '\n' }).FirstOrDefault();
                            #endregion

                            #region 4672，特權帳號
                            if (record.Id == 4672) { 

                                #region Subject Info
                                IREventLog[(int)IREvent.SubjectLogonId] = Properties[3].Value.ToString();
                                IREventLog[(int)IREvent.SubjectUserSid] = Properties[0].Value.ToString();
                                IREventLog[(int)IREvent.SubjectUserName] = Properties[1].Value.ToString();
                                IREventLog[(int)IREvent.SubjectDomainName] = Properties[2].Value.ToString();
                                IREventLog[(int)IREvent.Privilege] = Properties[4].Value.ToString();
                                #endregion                               

                            }
                            #endregion

                            #region 4624，登入事件
                            else if (record.Id == 4624) 
                            {
                                #region Subject Info
                                IREventLog[(int)IREvent.SubjectLogonId] = Properties[3].Value.ToString();
                                IREventLog[(int)IREvent.SubjectUserSid] = Properties[0].Value.ToString();
                                IREventLog[(int)IREvent.SubjectUserName] = Properties[1].Value.ToString();
                                IREventLog[(int)IREvent.SubjectDomainName] = Properties[2].Value.ToString();
                                #endregion

                                #region Target Info
                                IREventLog[(int)IREvent.TargetLogonId] = Properties[7].Value.ToString();
                                IREventLog[(int)IREvent.TargetUserSid] = Properties[4].Value.ToString();
                                IREventLog[(int)IREvent.TargetUserName] = Properties[5].Value.ToString();
                                IREventLog[(int)IREvent.TargetDomainName] = Properties[6].Value.ToString();
                                #endregion

                                #region Process Info
                                IREventLog[(int)IREvent.LogonType] = Properties[8].Value.ToString();

                                #region Logon Type
                                IREventLog[(int)IREvent.LogonTypeName] = getLogonTypeString(Properties[8].Value.ToString());
                                #endregion

                                IREventLog[(int)IREvent.ProcessID] = Properties[16].Value.ToString();
                                IREventLog[(int)IREvent.ProcessName] = Properties[17].Value.ToString();
                                #endregion

                                #region Network Info
                                IREventLog[(int)IREvent.SourceHost] = Properties[11].Value.ToString();
                                IREventLog[(int)IREvent.SourceIP] = Properties[18].Value.ToString();
                                IREventLog[(int)IREvent.SourcePort] = Properties[19].Value.ToString();
                                #endregion
                            }
                            #endregion

                            #region 4634，登出
                            else if (record.Id == 4634)
                            {
                                #region Subject Info
                                IREventLog[(int)IREvent.SubjectLogonId] = Properties[3].Value.ToString();
                                IREventLog[(int)IREvent.SubjectUserSid] = Properties[0].Value.ToString();
                                IREventLog[(int)IREvent.SubjectUserName] = Properties[1].Value.ToString();
                                IREventLog[(int)IREvent.SubjectDomainName] = Properties[2].Value.ToString();
                                #endregion                                

                                #region Process Info
                                IREventLog[(int)IREvent.LogonType] = Properties[4].Value.ToString();

                                #region Logon Type
                                IREventLog[(int)IREvent.LogonTypeName] = getLogonTypeString(Properties[4].Value.ToString());
                                #endregion
                                
                                #endregion                                
                            }
                            #endregion

                            #region 4625，登入失敗
                            else if (record.Id == 4625)
                            {
                                #region Subject Info
                                IREventLog[(int)IREvent.SubjectLogonId] = Properties[3].Value.ToString();
                                IREventLog[(int)IREvent.SubjectUserSid] = Properties[0].Value.ToString();
                                IREventLog[(int)IREvent.SubjectUserName] = Properties[1].Value.ToString();
                                IREventLog[(int)IREvent.SubjectDomainName] = Properties[2].Value.ToString();
                                #endregion

                                #region Target Info                                
                                IREventLog[(int)IREvent.TargetUserSid] = Properties[4].Value.ToString();
                                IREventLog[(int)IREvent.TargetUserName] = Properties[5].Value.ToString();
                                IREventLog[(int)IREvent.TargetDomainName] = Properties[6].Value.ToString();
                                #endregion

                                #region Process Info
                                IREventLog[(int)IREvent.LogonType] = Properties[10].Value.ToString();

                                #region Logon Type
                                IREventLog[(int)IREvent.LogonTypeName] = getLogonTypeString(Properties[10].Value.ToString());
                                #endregion

                                IREventLog[(int)IREvent.ProcessID] = Properties[17].Value.ToString();
                                IREventLog[(int)IREvent.ProcessName] = Properties[18].Value.ToString();
                                #endregion

                                #region Network Info
                                IREventLog[(int)IREvent.SourceHost] = Properties[13].Value.ToString();
                                IREventLog[(int)IREvent.SourceIP] = Properties[19].Value.ToString();
                                IREventLog[(int)IREvent.SourcePort] = Properties[20].Value.ToString();
                                #endregion

                                #region Auth Info
                                IREventLog[(int)IREvent.LogonProcess] = Properties[11].Value.ToString();
                                IREventLog[(int)IREvent.AuthPackage] = Properties[12].Value.ToString();
                                
                                #endregion
                            }
                            #endregion

                            #region 4648，登入事件(Schedule Task，RunAS)
                            else if (record.Id == 4672)
                            { 

                                #region Subject Info
                                IREventLog[(int)IREvent.SubjectLogonId] = Properties[3].Value.ToString();
                                IREventLog[(int)IREvent.SubjectUserSid] = Properties[0].Value.ToString();
                                IREventLog[(int)IREvent.SubjectUserName] = Properties[1].Value.ToString();
                                IREventLog[(int)IREvent.SubjectDomainName] = Properties[2].Value.ToString();                                
                                #endregion

                                #region Target Info                                
                                IREventLog[(int)IREvent.TargetUserName] = Properties[5].Value.ToString();
                                IREventLog[(int)IREvent.TargetDomainName] = Properties[6].Value.ToString();
                                #endregion

                                #region Process Info                                
                                IREventLog[(int)IREvent.ProcessID] = Properties[10].Value.ToString();
                                IREventLog[(int)IREvent.ProcessName] = Properties[11].Value.ToString();
                                #endregion

                                #region Network Info                                
                                IREventLog[(int)IREvent.SourceIP] = Properties[12].Value.ToString();
                                IREventLog[(int)IREvent.SourcePort] = Properties[13].Value.ToString();
                                #endregion
                            }
                            #endregion

                            #region 4776，登入 DC Auth by NTLM (The domain controller attempted to NTLM validate the credentials for an account)
                            else if (record.Id == 4776)
                            {
                                                                
                                #region Validate Method
                                IREventHeader[(int)IREvent.DCNtlmAuth] = Properties[0].Value.ToString();
                                #endregion 

                                #region Subject Info
                                IREventLog[(int)IREvent.SubjectUserName] = Properties[1].Value.ToString();
                                #endregion

                                #region Network Info
                                IREventLog[(int)IREvent.SourceHost] = Properties[2].Value.ToString();                                
                                #endregion

                                #region Error
                                IREventHeader[(int)IREvent.DCNtlmErrorCode] = Properties[3].Value.ToString();                                
                                IREventHeader[(int)IREvent.DCNtlmError] = get4776String((int)Properties[3].Value);
                                #endregion
                            }
                            #endregion

                            #region 4768，Kerberos認證服務(A Kerberos authentication ticket (TGT) was requested)
                            else if (record.Id == 4768)
                            {

                                #region Subject Info                                
                                IREventLog[(int)IREvent.SubjectUserSid] = Properties[2].Value.ToString();
                                IREventLog[(int)IREvent.SubjectUserName] = Properties[0].Value.ToString();
                                IREventLog[(int)IREvent.SubjectDomainName] = Properties[1].Value.ToString();
                                #endregion

                                #region Target Info
                                IREventLog[(int)IREvent.TargetUserName] = Properties[3].Value.ToString();
                                IREventLog[(int)IREvent.TargetUserSid] = Properties[4].Value.ToString();                                
                                #endregion

                                #region Network Info
                                string myIP = Properties[9].Value.ToString();
                                myIP = myIP.Replace("::ffff:", "");
                                IREventLog[(int)IREvent.SourceIP] = myIP;
                                IREventLog[(int)IREvent.SourcePort] = Properties[10].Value.ToString();
                                #endregion

                                #region Kerberos Info
                                IREventLog[(int)IREvent.KerberosTktOption] = Properties[5].Value.ToString();
                                IREventLog[(int)IREvent.KerberosErrorCode] = Properties[6].Value.ToString();
                                IREventLog[(int)IREvent.KerberosEncryptionType] = Properties[7].Value.ToString();
                                IREventLog[(int)IREvent.KerberosPreAuthenticationType] = Properties[8].Value.ToString();
                                #endregion
                            }
                            #endregion

                            #region 4769，Kerberos認證服務票證操作( A Kerberos service ticket was requested)
                            else if (record.Id == 4769)
                            {

                                #region Subject Info                                
                                IREventLog[(int)IREvent.SubjectUserName] = Properties[0].Value.ToString();
                                IREventLog[(int)IREvent.SubjectDomainName] = Properties[1].Value.ToString();
                                IREventLog[(int)IREvent.SubjectGUID] = Properties[9].Value.ToString();
                                #endregion

                                #region Target Info
                                IREventLog[(int)IREvent.TargetUserName] = Properties[2].Value.ToString();
                                IREventLog[(int)IREvent.TargetUserSid] = Properties[3].Value.ToString();
                                #endregion

                                #region Network Info
                                string myIP = Properties[6].Value.ToString();
                                myIP = myIP.Replace("::ffff:", "");
                                IREventLog[(int)IREvent.SourceIP] = myIP;
                                IREventLog[(int)IREvent.SourcePort] = Properties[7].Value.ToString();
                                #endregion

                                #region Kerberos Info
                                IREventLog[(int)IREvent.KerberosTktOption] = Properties[4].Value.ToString();
                                IREventLog[(int)IREvent.KerberosErrorCode] = Properties[8].Value.ToString();
                                IREventLog[(int)IREvent.KerberosEncryptionType] = Properties[5].Value.ToString();                                
                                #endregion
                            }
                            #endregion

                            dumpStrings2CSV(IREventLog);
                            #region Enable Dump Detailed Event Log
                            //dumpEventRecordSchema(record);
                            #endregion
                        }                        
                        
                        //Check label
                        //Console.WriteLine(meta.Description);                                                
                        
                    }                    
                }
            }
            Console.ReadKey();
        }

        static bool isFilterOut(EventRecord eventRecord)
        {
            bool isFilteredOut = false;
            IList<EventProperty> Properties = eventRecord.Properties;
            
            if (eventRecord.Id == 4624 &&  filteredEventCondition.ContainsKey(Properties[4].Value.ToString().Trim()))
            {
                isFilteredOut = true;
            }
            else if (eventRecord.Id == 4672 && filteredEventCondition.ContainsKey(Properties[0].Value.ToString().Trim()))
            {
                isFilteredOut = true;
            }

            return isFilteredOut;
        }

        static void dumpEventRecordSchema(EventRecord eventRecord)
        {
            var meta = new ProviderMetadata(eventRecord.ProviderName).Events.Where(evt => evt.Id == eventRecord.Id).FirstOrDefault();
            string myMeta = meta.Description;

            IList<EventProperty> Properties = eventRecord.Properties;
            Console.WriteLine("Event ID: " + eventRecord.Id);
            
            for (int i = Properties.Count  ; i >0 ; i--)
            {
                int myIndex = (i);
                string myKey = "%"+myIndex;
                myMeta = myMeta.Replace(myKey, "*" + myIndex + ": " + Properties[i - 1].Value);
            }
            
            Console.WriteLine(myMeta);

            //Console.WriteLine("[0]: " + Properties[0].Value);
            //Console.WriteLine("[1]: " + Properties[1].Value);
            //Console.WriteLine("[2]: " + Properties[2].Value);
        }

        static string getLogonTypeString(string pLogonType)
        {
            string LogonTypeString = "";

            #region Logon Type
            if (pLogonType.Equals("2"))
            {
                LogonTypeString = "Interactive(本機登入)";
            }
            else if (pLogonType.Equals("3"))
            {
                LogonTypeString = "Network(網路登入，Netbios、IIS等登入方式)";
            }
            else if (pLogonType.Equals("4"))
            {
                LogonTypeString = "Batch(排程等批次)";
            }
            else if (pLogonType.Equals("5"))
            {
                LogonTypeString = "Service(服務)";
            }
            else if (pLogonType.Equals("7"))
            {
                LogonTypeString = "Unlock(解除螢幕鎖定)";
            }
            else if (pLogonType.Equals("8"))
            {
                LogonTypeString = "NetworkCleartext(網路明文登入，IIS ASP登入)";
            }
            else if (pLogonType.Equals("9"))
            {
                LogonTypeString = "NewCredentials(新身份登入，通常為RunAs方式)";
            }
            else if (pLogonType.Equals("10"))
            {
                LogonTypeString = "RemoteInteractive(遠端登入，例如Terminal Server、RDP等)";
            }
            #endregion

            return LogonTypeString;
        }

        static string get4776String(int pErrorCode)
        {
            string Result = "";

            if (pErrorCode == 0x64)
            {
                Result = "user name does not exist";
            }
            else if (pErrorCode == 0x6A)
            {
                Result = "user name is correct but the password is wrong";
            }
            else if (pErrorCode == 0x234)
            {
                Result = "user is currently locked out";
            }
            else if (pErrorCode == 0x72)
            {
                Result = "account is currently disabled";
            }
            else if (pErrorCode == 0x6F)
            {
                Result = "user tried to logon outside his day of week or time of day restrictions";
            }
            else if (pErrorCode == 0x70)
            {
                Result = "workstation restriction";
            }
            else if (pErrorCode == 0x193)
            {
                Result = "account expiration";
            }
            else if (pErrorCode == 0x71)
            {
                Result = "expired password";
            }
            else if (pErrorCode == 0x224)
            {
                Result = "user is required to change password at next logon";
            }
            else if (pErrorCode == 0x225)
            {
                Result = "evidently a bug in Windows and not a risk";
            }

            return Result;
        }
    }
}
