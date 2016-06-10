namespace Extreme.Core
{
    public interface IProfiler
    {
        void Start(int code);
        void End(int code);
    }
}
