using SimpleLogger.Helpers;

namespace SimpleLogger
{
    public class NamedLogger : ILogger
    {
        private readonly ILogger logger;
        private readonly string name;

        public NamedLogger(ILogger logger, string name)
        {
            if (string.IsNullOrEmpty(name)) throw new System.ArgumentException($"'{nameof(name)}' cannot be null or empty.", nameof(name));
            this.logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
            this.name = name;
        }

        public void Info(string text, string overrideName = null)
        {
            logger.Info(text, overrideName ?? name);
        }

        public void Warn(string text, string overrideName = null)
        {
            logger.Warn(text, overrideName ?? name);
        }

        public void Error(string text, string overrideName = null)
        {
            logger.Error(text, overrideName ?? name);
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
            return logger.GetEntriesCount(type);
        }

    }
}
