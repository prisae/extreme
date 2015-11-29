namespace Extreme.Core.Logger
{
    public class EmptyLogger : ILogger
    {
        public void Write(int logLevvel, string message)
        {
        }

        public void Dispose()
        {
            }
    }
}
