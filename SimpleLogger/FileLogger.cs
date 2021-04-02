using SimpleLogger.Helpers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;

namespace SimpleLogger
{
    public class FileLogger : ILoggerDisposable
    {
        private readonly Subject<LogEntry> subject = new Subject<LogEntry>();
        private readonly StringBuilder stringBuilder = new StringBuilder();
        private readonly string Filename;
        private bool disposedValue;
        private readonly ConcurrentDictionary<LogEntryType, long> stats = new ConcurrentDictionary<LogEntryType, long>();
        
        public FileLogger(string filename, uint bufferLengthMs, Action<Exception, IEnumerable<string>> onError)
        {
            if (string.IsNullOrWhiteSpace(filename)) throw new ArgumentException($"'{nameof(filename)}' cannot be null or whitespace.", nameof(filename));

            Filename = filename;
            subject
               .Buffer(TimeSpan.FromMilliseconds(bufferLengthMs))
               .Subscribe(logEntries =>
               {
                   try
                   {
                       stringBuilder.Clear();
                       foreach (LogEntry logEntry in logEntries)
                           if (logEntry.LogName != null)
                               stringBuilder.AppendLine($"{logEntry.Time:dd.MM.yyyy HH:mm:ss.fff} [{logEntry.Type}] [{logEntry.LogName}] {logEntry.Text}");
                           else
                               stringBuilder.AppendLine($"{logEntry.Time:dd.MM.yyyy HH:mm:ss.fff} [{logEntry.Type}] {logEntry.Text}");
                       if (stringBuilder.Length > 0)
                           File.AppendAllText(Filename, stringBuilder.ToString(), Encoding.UTF8);
                       stringBuilder.Clear();
                   }
                   catch (Exception ex)
                   {
                       onError?.Invoke(ex, logEntries.Select(x => x.Text));
                   }
               });
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
