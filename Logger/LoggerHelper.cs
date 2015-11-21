namespace Extreme.Core
{
    public static class LoggerHelper
    {
        public static void WriteStatus(this ILogger logger, string message)
        {
            logger.Write((int) LogLevel.Status, message);
        }

        public static void WriteWarning(this ILogger logger, string message)
        {
            logger.Write((int) LogLevel.Warning, message);
        }

        public static void WriteError(this ILogger logger, string message)
        {
            logger.Write((int) LogLevel.Error, message);
        }
    }

}
