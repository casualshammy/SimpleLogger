using System;

namespace SimpleLogger.Helpers
{
    internal class LogEntry
    {
        internal LogEntryType Type;
        internal string Text;
        internal DateTime Time;
        internal string LogName;

        internal LogEntry(LogEntryType type, string text, DateTime time, string logName)
        {
            Type = type;
            Text = text;
            Time = time;
            LogName = logName;
        }
    }
}
