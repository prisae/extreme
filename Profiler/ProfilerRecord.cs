using System;
using System.Diagnostics;

namespace Extreme.Core
{
    public class ProfilerRecord
    {
        private readonly int _code;
        private readonly long _timeStamp;
        private readonly bool _isStart;

        public ProfilerRecord(int code, long timeStamp, bool isStart)
        {
            _code = code;
            _timeStamp = timeStamp;
            _isStart = isStart;
        }

        public int Code
        {
            get { return _code; }
        }

        public long TimeStamp
        {
            get { return _timeStamp; }
        }

        public bool IsStart
        {
            get { return _isStart; }
        }
        
        public bool IsEnd
        {
            get { return !_isStart; }
        }

        public static ProfilerRecord NewStart(int code)
        {
            return new ProfilerRecord(code, Stopwatch.GetTimestamp(), true);
        }

        public static ProfilerRecord NewEnd(int code)
        {
            return new ProfilerRecord(code, Stopwatch.GetTimestamp(), false);
        }
    }
}
