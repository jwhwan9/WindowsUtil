using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics.Eventing.Reader;
using System.Text.RegularExpressions;

namespace QMsSQL_AppEvnet
{
    class Program
    {
        #region Common Variable
        public static Dictionary<int, int> filteredEventID;
        public static Dictionary<string, string> filteredEventCondition;
        public static string EventLogFile = @"Application.evtx";
        public static string[] IREventHeader;

        enum IREvent
        {
            DateTime, MachineName, RecordID, Level, LogName, EventID, Task, TaskID, AccountType, AccountName, ClientIP, Provider, Brief, Misc
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
            IREventHeader[(int)IREvent.Level] = "Level";
            IREventHeader[(int)IREvent.LogName] = "LogName";
            IREventHeader[(int)IREvent.EventID] = "EventID";
            IREventHeader[(int)IREvent.Task] = "Task";
            IREventHeader[(int)IREvent.TaskID] = "TaskID";
            IREventHeader[(int)IREvent.Provider] = "Provider";
            IREventHeader[(int)IREvent.Brief] = "Brief";
            IREventHeader[(int)IREvent.Misc] = "Misc";
            IREventHeader[(int)IREvent.AccountType] = "AccountType";
            IREventHeader[(int)IREvent.AccountName] = "AccountName";
            IREventHeader[(int)IREvent.ClientIP] = "ClientIP";
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
                if (i < (pStrings.Length - 1))
                {
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
            }

            if (!File.Exists(EventLogFile))
            {
                Console.WriteLine("usage:");
                Console.WriteLine("QMsSQL_AppEvent.exe <Application.evtx>");
                Environment.Exit(0);
            }

            NewIREventString(true);

            filteredEventID = new Dictionary<int, int>();
            filteredEventCondition = new Dictionary<string, string>();

            filteredEventID = new System.Collections.Generic.Dictionary<int, int>();


            // Enriched Event               


            filteredEventID.Add(18454, 18454); // SQL Account Login MS-SQL
            filteredEventID.Add(18453, 18453); // Domain Account Login MS-SQL


            filteredEventCondition = new System.Collections.Generic.Dictionary<string, string>();

            using (var reader = new EventLogReader(EventLogFile, PathType.FilePath))
            {
                EventRecord record;

                reader.BatchSize = 256;

                int i = 0;
                while ((record = reader.ReadEvent()) != null)
                {
                    #region begin parse event
                    try
                    {

                        using (record)
                        {
                            if (!filteredEventID.ContainsKey(record.Id)) continue;

                            var meta = new ProviderMetadata(record.ProviderName).Events.Where(evt => evt.Id == record.Id).FirstOrDefault();
                            IList<EventProperty> Properties = record.Properties;

                            if (!isFilterOut(record))
                            {

                                string[] IREventLog = new string[Enum.GetNames(typeof(IREvent)).Length];

                                #region Common Event Info
                                IREventLog[(int)IREvent.DateTime] = record.TimeCreated.Value.ToString("u");
                                IREventLog[(int)IREvent.MachineName] = record.MachineName;
                                IREventLog[(int)IREvent.RecordID] = record.RecordId.ToString();
                                IREventLog[(int)IREvent.LogName] = record.LogName;
                                IREventLog[(int)IREvent.Level] = record.LevelDisplayName;
                                IREventLog[(int)IREvent.EventID] = record.Id.ToString();
                                IREventLog[(int)IREvent.Task] = record.TaskDisplayName;
                                IREventLog[(int)IREvent.TaskID] = record.Task.ToString();
                                IREventLog[(int)IREvent.Provider] = record.ProviderName.ToString();

                                try
                                {
                                    IREventLog[(int)IREvent.Brief] = record.FormatDescription().Replace(System.Environment.NewLine, "<br>");//record.FormatDescription().Split(new[] { '\r', '\n' }).FirstOrDefault();
                                }
                                catch (Exception ex)
                                {
                                    IREventLog[(int)IREvent.Brief] = record.FormatDescription();
                                }
                                #endregion

                                #region 18453，Domain Account
                                if (record.Id == 18453)
                                {

                                    #region Subject Info
                                    IREventLog[(int)IREvent.AccountType] = "Domain";
                                    string sentence = IREventLog[(int)IREvent.Brief];
                                    string[] wkWords = Regex.Split(sentence, @"\W");
                                    string[] wkIPs = sentence.Split(new string[] { "CLIENT: " }, StringSplitOptions.None);
                                    
                                    // Match all quoted fields
                                    MatchCollection col = Regex.Matches(sentence, @"'(.*?)'");

                                    IREventLog[(int)IREvent.AccountName] = col[0].Groups[1].Value;

                                    IREventLog[(int)IREvent.ClientIP] = wkIPs[1].Replace("]", "");
                                    #endregion

                                }
                                #endregion

                                #region 18454，SQL Account
                                else if (record.Id == 18454)
                                {

                                    #region Subject Info
                                    IREventLog[(int)IREvent.AccountType] = "SQL";
                                    string sentence = IREventLog[(int)IREvent.Brief];
                                    string[] wkWords = Regex.Split(sentence, @"\W");
                                    string[] wkIPs = sentence.Split(new string[]{"CLIENT: "}, StringSplitOptions.None);
                                    IREventLog[(int)IREvent.AccountName] = wkWords[2];
                                    //IREventLog[(int)IREvent.ClientIP] = wkIPs[1].Replace("]","");
                                    // Match all quoted fields
                                    MatchCollection col = Regex.Matches(sentence, @"[CLIENT: (.*?)]");
                                    IREventLog[(int)IREvent.ClientIP] = col[0].Groups[1].Value;
                                    #endregion

                                }
                                #endregion


                                dumpStrings2CSV(IREventLog);
                                #region Enable Dump Detailed Event Log
                                //dumpEventRecordSchema(record);
                                #endregion
                            }


                        }

                    }
                    catch (Exception ex)
                    {

                    }
                    #endregion
                }

            }
        }

        static bool isFilterOut(EventRecord eventRecord)
        {
            bool isFilteredOut = false;
            IList<EventProperty> Properties = eventRecord.Properties;

            if (eventRecord.Id == 4624 && filteredEventCondition.ContainsKey(Properties[4].Value.ToString().Trim()))
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
            if (eventRecord.ProviderName == null) return;
            var meta = new ProviderMetadata(eventRecord.ProviderName).Events.Where(evt => evt.Id == eventRecord.Id).FirstOrDefault();

            if (meta == null) return;
            string myMeta = meta.Description;

            IList<EventProperty> Properties = eventRecord.Properties;
            Console.WriteLine("Event ID: " + eventRecord.Id);

            for (int i = Properties.Count; i > 0; i--)
            {
                int myIndex = (i);
                string myKey = "%" + myIndex;
                myMeta = myMeta.Replace(myKey, "*" + myIndex + ": " + Properties[i - 1].Value);
            }

            Console.WriteLine(myMeta);

            //Console.WriteLine("[0]: " + Properties[0].Value);
            //Console.WriteLine("[1]: " + Properties[1].Value);
            //Console.WriteLine("[2]: " + Properties[2].Value);
        }
    }
}
