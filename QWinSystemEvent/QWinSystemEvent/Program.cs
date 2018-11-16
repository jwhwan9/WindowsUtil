using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics.Eventing.Reader;

namespace QWinSystemEvent
{
    class Program
    {
        #region Common Variable
        public static Dictionary<int, int> filteredEventID;
        public static Dictionary<string, string> filteredEventCondition;
        public static string EventLogFile = @"System.evtx";
        public static string[] IREventHeader;

        enum IREvent
        {
            DateTime, MachineName, RecordID, Level, LogName, EventID, Task, TaskID, Provider, Brief, Misc
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
                Console.WriteLine("QWinSystemEvent.exe <System.evtx>");
                Environment.Exit(0);
            }

            NewIREventString(true);

            filteredEventID = new Dictionary<int, int>();
            filteredEventCondition = new Dictionary<string, string>();

            filteredEventID = new System.Collections.Generic.Dictionary<int, int>();


            // Enriched Event               
            
            filteredEventID.Add(12, 12); // PC Power on.           
            filteredEventID.Add(13, 13); // PC Power off.           

            filteredEventID.Add(18, 18); // Windows Update Downloaded
            filteredEventID.Add(19, 19); // Windows Update Installed
            filteredEventID.Add(27, 27); // Windows Update Paused    

            filteredEventID.Add(37, 37); // Time Service

            filteredEventID.Add(41, 41); // Kernel Power

            filteredEventID.Add(1502, 1502); // Group Policy OK
            
            filteredEventID.Add(1030, 1030); // Group Policy Failed
            
            filteredEventID.Add(6005, 6005); // EventLog
            filteredEventID.Add(6006, 6006); // EventLog Stop
            filteredEventID.Add(6008, 6008); // EventLog 意外關機
            filteredEventID.Add(6013, 6013); // EventLog System Start Time

            filteredEventID.Add(7000, 7000); // System Error
            //filteredEventID.Add(7036, 7036); // Service Status
            filteredEventID.Add(7023, 7023); // System Error

            filteredEventID.Add(45058, 45058); // 登入快取項目是最舊的項目而且已經移除
            
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

                                #region 12，電腦開機
                                if (record.Id == 12)
                                {

                                    #region Subject Info
                                    IREventLog[(int)IREvent.Task] = "電腦開機";

                                    #endregion

                                }
                                #endregion

                                #region 13，電腦關機
                                else if (record.Id == 13)
                                {

                                    #region Subject Info
                                    IREventLog[(int)IREvent.Task] = "電腦關機";

                                    #endregion

                                }
                                #endregion
                                #region 6013，Uptime
                                else if (record.Id == 6013)
                                {

                                    #region Subject Info
                                    IREventLog[(int)IREvent.Task] = "Uptime";

                                    #endregion

                                }
                                #endregion
                                #region 1502，GPO.OK
                                else if (record.Id == 1502)
                                {

                                    #region Subject Info
                                    IREventLog[(int)IREvent.Task] = "GPO.OK";

                                    #endregion

                                }
                                #endregion
                                #region 1030，GPO.Failed
                                else if (record.Id == 1030)
                                {

                                    #region Subject Info
                                    IREventLog[(int)IREvent.Task] = "GPO.Failed";

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
