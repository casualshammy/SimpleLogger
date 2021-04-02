using SimpleLogger.Helpers;

namespace SimpleLogger
{
    public static class Extensions
    {
        public static NamedLogger GetNamedLog(this ILogger logger, string name)
        {
            return new NamedLogger(logger, name);
        }
    }
}
