using SimpleLogger.Helpers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Timers;

namespace SimpleLogger
{
    public class FileLogger : ILoggerDisposable
    {
        private readonly ConcurrentQueue<LogEntry> buffer = new ConcurrentQueue<LogEntry>();
        private readonly string Filename;
        private bool disposedValue;
        private readonly ConcurrentDictionary<LogEntryType, long> stats = new ConcurrentDictionary<LogEntryType, long>();
        private readonly Timer timer;
        private readonly Action<Exception, IEnumerable<string>> onErrorHandler;

        public FileLogger(string filename, uint bufferLengthMs, Action<Exception, IEnumerable<string>> onError)
        {
            if (string.IsNullOrWhiteSpace(filename)) 
                throw new ArgumentException($"'{nameof(filename)}' cannot be null or whitespace.", nameof(filename));

            onErrorHandler = onError;
            Filename = filename;
            timer = new Timer(bufferLengthMs);
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
        }

        public double BufferLengthMs
        {
            get => timer.Interval;
            set => timer.Interval = value;
        }

        public void Info(string text, string name = null)
        {
            if (text is null) 
                throw new ArgumentNullException(nameof(text));

            stats.AddOrUpdate(LogEntryType.INFO, 1, (_, prevValue) => ++prevValue);

            buffer.Enqueue(new LogEntry(LogEntryType.INFO, text, DateTime.UtcNow, name));
        }

        public void Error(string text, string name = null)
        {
            if (text is null) 
                throw new ArgumentNullException(nameof(text));

            stats.AddOrUpdate(LogEntryType.ERROR, 1, (_, prevValue) => ++prevValue);

            buffer.Enqueue(new LogEntry(LogEntryType.ERROR, text, DateTime.UtcNow, name));
        }

        public void Warn(string text, string name = null)
        {
            if (text is null) 
                throw new ArgumentNullException(nameof(text));

            stats.AddOrUpdate(LogEntryType.WARN, 1, (_, prevValue) => ++prevValue);

            buffer.Enqueue(new LogEntry(LogEntryType.WARN, text, DateTime.UtcNow, name));
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

        public NamedLogger this[string name] => new NamedLogger(this, name);

        protected virtual void Dispose(bool disposing)
        {
           if (!disposedValue)
           {
              if (disposing)
              {
                    timer.Dispose();
                    Flush();
              }
              disposedValue = true;
           }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public void Flush()
        {
            try
            {
                StringBuilder stringBuilder = new StringBuilder();
                while (buffer.TryDequeue(out var logEntry))
                {
                    if (logEntry.LogName != null)
                        stringBuilder.AppendLine($"{logEntry.Time:dd.MM.yyyy HH:mm:ss.fff} [{logEntry.Type}] [{logEntry.LogName}] {logEntry.Text}");
                    else
                        stringBuilder.AppendLine($"{logEntry.Time:dd.MM.yyyy HH:mm:ss.fff} [{logEntry.Type}] {logEntry.Text}");
                }

                if (stringBuilder.Length > 0)
                    File.AppendAllText(Filename, stringBuilder.ToString(), Encoding.UTF8);
                stringBuilder.Clear();
            }
            catch (Exception ex)
            {
                onErrorHandler?.Invoke(ex, buffer.Select(l => l.Text));
            }
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Flush();
        }

    }
}
