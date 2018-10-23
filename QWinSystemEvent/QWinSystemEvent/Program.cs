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
            DateTime, MachineName, RecordID, LogName, EventID, Task, TaskID, Provider, Brief
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

            filteredEventID.Add(45058, 45058); // 登入快取項目是最舊的項目而且已經移除

            filteredEventCondition = new System.Collections.Generic.Dictionary<string, string>();

            using (var reader = new EventLogReader(EventLogFile, PathType.FilePath))
            {
                EventRecord record;

                reader.BatchSize = 256;

                int i = 0;
                while ((record = reader.ReadEvent()) != null)
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
                            IREventLog[(int)IREvent.EventID] = record.Id.ToString();
                            IREventLog[(int)IREvent.Task] = record.TaskDisplayName;
                            IREventLog[(int)IREvent.TaskID] = record.Task.ToString();
                            IREventLog[(int)IREvent.Provider] = record.ProviderName.ToString();
                            try
                            {
                                IREventLog[(int)IREvent.Brief] = record.FormatDescription().Split(new[] { '\r', '\n' }).FirstOrDefault();
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

                            dumpStrings2CSV(IREventLog);
                            #region Enable Dump Detailed Event Log
                            //dumpEventRecordSchema(record);
                            #endregion
                        }


                    }
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
            var meta = new ProviderMetadata(eventRecord.ProviderName).Events.Where(evt => evt.Id == eventRecord.Id).FirstOrDefault();
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
