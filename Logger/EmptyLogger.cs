namespace Extreme.Core
{
    public class EmptyLogger : ILogger
    {
        public void Write(int logLevvel, string message)
        {
        }
    }
}
