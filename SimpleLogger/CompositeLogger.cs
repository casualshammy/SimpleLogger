using SimpleLogger.Helpers;
using System;
using System.Collections.Concurrent;

namespace SimpleLogger
{
    public class CompositeLogger : ILoggerDisposable
    {
        private readonly ILoggerDisposable[] Loggers;
        private bool disposedValue;
        private readonly ConcurrentDictionary<LogEntryType, long> stats = new ConcurrentDictionary<LogEntryType, long>();

        public CompositeLogger(params ILoggerDisposable[] loggers)
        {
            Loggers = loggers;
        }

        public void Error(string text, string name = null)
        {
            stats.AddOrUpdate(LogEntryType.ERROR, 1, (_, prevValue) => ++prevValue);
            foreach (ILogger logger in Loggers)
                logger.Error(text, name);
        }

        public void Info(string text, string name = null)
        {
            stats.AddOrUpdate(LogEntryType.INFO, 1, (_, prevValue) => ++prevValue);
            foreach (ILogger logger in Loggers)
                logger.Info(text, name);
        }

        public void Warn(string text, string name = null)
        {
            stats.AddOrUpdate(LogEntryType.WARN, 1, (_, prevValue) => ++prevValue);
            foreach (ILogger logger in Loggers)
                logger.Warn(text, name);
        }

        public void NewEvent(LogEntryType type, string text)
        {
            foreach (ILogger logger in Loggers)
                logger.NewEvent(type, text);
        }

        public long GetEntriesCount(LogEntryType type)
        {
            stats.TryGetValue(type, out long value);
            return value;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                    foreach (var logger in Loggers)
                        logger.Dispose();
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            System.GC.SuppressFinalize(this);
        }

    }
}
