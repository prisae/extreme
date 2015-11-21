using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Extreme.Core
{
    public class ProfilerStatistics
    {
        public TimeSpan[] Times { get; }

        public int TotalNumber => Times.Length;

        public int Code { get; }

        public TimeSpan TotalTime { get; private set; }

        public TimeSpan Min { get; private set; }

        public TimeSpan Max { get; private set; }

        public TimeSpan Mean { get; private set; }

        public TimeSpan StandardDeviation { get; private set; }

        private ProfilerStatistics(int code, TimeSpan[] times)
        {
            Code = code;
            Times = times;
        }

        public static ProfilerStatistics CalculateStatistics(int code, List<TimeSpan> times)
        {
            var result = new ProfilerStatistics(code, times.ToArray());

            var mean = times.Average(t => t.Ticks);

            double variance = 0;

            for (int i = 0; i < times.Count; i++)
                variance += ((times[i].Ticks - mean) * (times[i].Ticks - mean)) / times.Count;

            var stdDev = Math.Sqrt(variance);

            result.TotalTime = new TimeSpan(times.Sum(ts => ts.Ticks));
            result.Mean = new TimeSpan((long)mean);
            result.Min = times.Min();
            result.Max = times.Max();
            result.StandardDeviation = new TimeSpan((long)stdDev);

            return result;
        }
    }
}
