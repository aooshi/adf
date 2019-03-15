using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Adf
{
    /// <summary>
    /// Windows Event Log
    /// </summary>
    public static class WindowsEventLog
    {
        /// <summary>
        /// New Log
        /// </summary>
        /// <param name="type"></param>
        /// <param name="content"></param>
        /// <param name="args"></param>
        public static void NewLog(EventLogEntryType type, string content, params object[] args)
        {
            var name = Process.GetCurrentProcess().ProcessName;

            using (var eventLog = new EventLog("Application"))
            {
                eventLog.Source = name;
                eventLog.WriteEntry(string.Format(content,args), type);
            }
        }
    }
}
