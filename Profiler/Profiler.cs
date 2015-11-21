using System;
using System.Collections.Generic;

namespace Extreme.Core
{
    public class Profiler : IProfiler
    {
        private readonly List<ProfilerRecord> _records = new List<ProfilerRecord>();
        
        public void Start(int code)
            => _records.Add(ProfilerRecord.NewStart(code));

        public void End(int code)
            => _records.Add(ProfilerRecord.NewEnd(code));

        public ProfilerRecord[] GetAllRecords()
            => _records.ToArray();

        public void ClearAllRecords()
            => _records.Clear();
    }
}
