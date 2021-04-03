using System;

namespace SimpleLogger.Helpers
{
    public interface ILogger
    {
        void Info(string text, string name = null);

        void Error(string text, string name = null);

        void Warn(string text, string name = null);

        void NewEvent(LogEntryType type, string text);

        long GetEntriesCount(LogEntryType type);

        void Flush();

    }

    public interface ILoggerDisposable : ILogger, IDisposable { }

}
