namespace Extreme.Core
{
    public interface ILogger
    {
        void Write(int logLevel, string message);
    }
}
