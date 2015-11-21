using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extreme.Core
{
    public class ProfilerStatisticsAnalyzer
    {
        private readonly ProfilerRecord[] _records;


        public ProfilerStatisticsAnalyzer(ProfilerRecord[] records)
        {
            if (records == null) throw new ArgumentNullException("records");

            _records = records;
        }

        public ProfilerStatistics[] PerformAnalysis()
        {
            var dict = CreateDictionary();

            var stats = dict.Select(kvp => ProfilerStatistics.CalculateStatistics(kvp.Key, kvp.Value));

            return stats.ToArray();
        }

        public string ConvertToString1()
        {
            var statistics = PerformAnalysis();

            return ConvertToString1(statistics);
        }

        public string ConvertToString1(ProfilerStatistics[] statistics)
        {
            var sb = new StringBuilder();

            foreach (var stat in statistics)
            {
                var code = stat.Code;

                var pe = (ProfilerEvent)code;
                var isDefined = Enum.IsDefined(typeof(ProfilerEvent), code);

                string head = string.Format("{1} (code {0}) ", code, isDefined ? pe.ToString() : "");

                sb.AppendFormat("----{0}---------\n", head.PadLeft(40, '-'));
                sb.AppendFormat("total number of calls      {0}\n", stat.TotalNumber);
                sb.AppendFormat("total time          {0}\n", stat.TotalTime);
                sb.AppendFormat("min   time          {0}\n", stat.Min);
                sb.AppendFormat("max   time          {0}\n", stat.Max);
                sb.AppendFormat("mean  time          {0}\n", stat.Mean);
                sb.AppendFormat("standard deviation  {0}\n", stat.StandardDeviation);
                sb.AppendFormat("\n");
            }

            return sb.ToString();
        }

        public string ConvertToString2()
        {
            var statistics = PerformAnalysis();

            return ConvertToString2(statistics);
        }

        public string ConvertToString2(ProfilerStatistics[] statistics)
        {
            var sb = new StringBuilder();

            sb.AppendFormat("{0};", PadRight(40, "name"));
            sb.AppendFormat("{0};", PadLeft(5, "code"));
            sb.AppendFormat("{0};", PadLeft(5, "tNum"));
            sb.AppendFormat("{0};", PadLeft(20, "tTime"));
            sb.AppendFormat("{0};", PadLeft(20, "min"));
            sb.AppendFormat("{0};", PadLeft(20, "max"));
            sb.AppendFormat("{0};", PadLeft(20, "avg"));
            sb.AppendFormat("{0};", PadLeft(20, "std"));
            sb.AppendFormat("\n");

            foreach (var stat in statistics)
            {
                var code = stat.Code;

                var pe = ((ProfilerEvent)code).ToString();

                sb.AppendFormat("{0};", PadRight(40, pe));
                sb.AppendFormat("{0};", PadLeft(code));
                sb.AppendFormat("{0};", PadLeft(stat.TotalNumber));
                sb.AppendFormat("{0};", PadLeft(stat.TotalTime));
                sb.AppendFormat("{0};", PadLeft(stat.Min));
                sb.AppendFormat("{0};", PadLeft(stat.Max));
                sb.AppendFormat("{0};", PadLeft(stat.Mean));
                sb.AppendFormat("{0};", PadLeft(stat.StandardDeviation));
                sb.AppendFormat("\n");
            }

            return sb.ToString();
        }

        private string PadLeft(TimeSpan timeSpan)
        {
            return timeSpan.ToString().PadLeft(20, ' ');
        }
        private string PadLeft(int code)
        {
            return code.ToString().PadLeft(5, ' ');
        }

        private string PadLeft(int legth, string name)
        {
            return name.PadLeft(legth, ' ');
        }
        private string PadRight(int legth, string name)
        {
            return name.PadRight(legth, ' ');
        }


        private Dictionary<int, List<TimeSpan>> CreateDictionary()
        {
            var dict = new Dictionary<int, List<TimeSpan>>();

            for (int i = 0; i < _records.Length; i++)
            {
                var record = _records[i];

                if (record.IsEnd)
                    continue;

                var endRecord = FindEndRecord(i, record.Code);

                if (endRecord == null)
                    throw new InvalidOperationException(string.Format("Unbalanced profiler record [{0}]", (ProfilerEvent)record.Code));

                var start = record.TimeStamp;
                var end = endRecord.TimeStamp;


                if (!dict.ContainsKey(record.Code))
                    dict.Add(record.Code, new List<TimeSpan>());

                dict[record.Code].Add(ConvertStopwatchTicksToTimeSpan(end - start));
            }

            return dict;
        }

        // From Stopwatch
        private static TimeSpan ConvertStopwatchTicksToTimeSpan(long ticks)
        {
            long newTicks = ticks;

            const long TicksPerMillisecond = 10000;
            const long TicksPerSecond = TicksPerMillisecond * 1000;

            if (Stopwatch.IsHighResolution)
            {
                var tickFrequency = TicksPerSecond / Stopwatch.Frequency;
                
                // convert high resolution perf counter to DateTime ticks
                double dticks = ticks;
                dticks *= tickFrequency;
                newTicks = unchecked((long)dticks);
            }

            return new TimeSpan(newTicks);
        }


        private ProfilerRecord FindEndRecord(int startIndex, int code)
        {
            for (int i = startIndex + 1; i < _records.Length; i++)
            {
                var rec = _records[i];

                if (rec.Code == code && rec.IsEnd)
                    return rec;
            }

            return null;
        }
    }
}
