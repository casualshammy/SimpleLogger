using SimpleLogger.Helpers;
using System;
using System.Collections.Concurrent;
using System.Reactive.Subjects;

namespace SimpleLogger
{
    public class ConsoleLogger : ILoggerDisposable
    {
        private readonly Subject<LogEntry> subject = new Subject<LogEntry>();
        private bool disposedValue;
        private readonly ConcurrentDictionary<LogEntryType, long> stats = new ConcurrentDictionary<LogEntryType, long>();

        public ConsoleLogger()
        {
            subject
               .Subscribe(logEntry =>
               {
                   string text = null;
                   if (logEntry.LogName != null)
                       text = $"{logEntry.Time:dd.MM.yyyy HH:mm:ss.fff} [{logEntry.Type}] [{logEntry.LogName}] {logEntry.Text}\n";
                   else
                       text = $"{logEntry.Time:dd.MM.yyyy HH:mm:ss.fff} [{logEntry.Type}] {logEntry.Text}\n";
                   if (logEntry.Type == LogEntryType.INFO)
                       ConsoleWriteColourText(text, ConsoleColor.White);
                   else if (logEntry.Type == LogEntryType.WARN)
                       ConsoleWriteColourText(text, ConsoleColor.Yellow);
                   else if (logEntry.Type == LogEntryType.ERROR)
                       ConsoleWriteColourText(text, ConsoleColor.Red);
               });
        }

        private static void ConsoleWriteColourText(string text, ConsoleColor colour)
        {
            ConsoleColor oldColour = Console.ForegroundColor;
            Console.ForegroundColor = colour;
            Console.Write(text);
            Console.ForegroundColor = oldColour;
        }

        public void Info(string text, string name = null)
        {
            if (text is null) throw new ArgumentNullException(nameof(text));
            stats.AddOrUpdate(LogEntryType.INFO, 1, (_, prevValue) => ++prevValue);
            subject.OnNext(new LogEntry(LogEntryType.INFO, text, DateTime.UtcNow, name));
        }

        public void Error(string text, string name = null)
        {
            if (text is null) throw new ArgumentNullException(nameof(text));
            stats.AddOrUpdate(LogEntryType.ERROR, 1, (_, prevValue) => ++prevValue);
            subject.OnNext(new LogEntry(LogEntryType.ERROR, text, DateTime.UtcNow, name));
        }

        public void Warn(string text, string name = null)
        {
            if (text is null) throw new ArgumentNullException(nameof(text));
            stats.AddOrUpdate(LogEntryType.WARN, 1, (_, prevValue) => ++prevValue);
            subject.OnNext(new LogEntry(LogEntryType.WARN, text, DateTime.UtcNow, name));
        }

        public void NewEvent(LogEntryType type, string text)
        {
            if (type == LogEntryType.INFO)
                Info(text);
            else if (type == LogEntryType.WARN)
                Warn(text);
            else if (type == LogEntryType.ERROR)
                Error(text);
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
                {
                    subject.OnCompleted();
                    subject.Dispose();
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
