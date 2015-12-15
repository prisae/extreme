using System;
using System.Globalization;
using System.IO;
using System.Linq;
using Extreme.Cartesian.Model;
using Extreme.Core;

namespace Profiling
{
    public class ProfilerResultsTextExporter
    {
        private readonly ProfilerStatistics[] _analisisResult;
        private readonly CartesianModel _model;

        private ProfilerResultsTextExporter(ProfilerStatistics[] analisisResult, CartesianModel model)
        {
            _analisisResult = analisisResult;
            _model = model;
        }

        public static void SaveRawProfilingResults(string path, CartesianModel cartesianModel, Profiler profiler)
        {
            profiler.GetAllRecords().SaveWithModel(path, cartesianModel);
        }

        public static void SaveProfilingResultsTo(string path, CartesianModel model, ProfilerStatistics[] analisisResult, int nMpi, int nThreads)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            if (analisisResult == null) throw new ArgumentNullException(nameof(analisisResult));
            if (model == null) throw new ArgumentNullException(nameof(model));

            new ProfilerResultsTextExporter(analisisResult, model)
            .SaveProfilingResultsTo(path, nMpi, nThreads);
        }

        private void SaveProfilingResultsTo(string path, int nMpi, int nThreads)
        {
            double percentSumm = 0;

            using (var sw = new StreamWriter(path))
            {
                WriteModelInfo(sw, nMpi, nThreads);
                WriteIterationsInfo(sw);

                WriteInfoAboutCustomFft(sw);

                percentSumm += WriteInfoAboutTensorAtoA(sw);
                percentSumm += WriteInfoAboutCie(sw);
                percentSumm += WriteInfoObservations(sw);
                percentSumm += WriteInfoAboutMisc(sw);

                sw.WriteLine("\n\n                                               Total Covered: {0:F1} %", percentSumm);
            }
        }

        private void WriteIterationsInfo(StreamWriter sw)
        {
            var dotProduct = _analisisResult.FirstOrDefault(a => a.Code == (int)ProfilerEvent.CalcDotProduct);
            var mult = _analisisResult.FirstOrDefault(a => a.Code == (int)ProfilerEvent.OperatorAMultiplication);

            sw.WriteLine();
            if (mult != null)
                sw.WriteLine($"Total number of multiplications: {mult.TotalNumber}");

            if (dotProduct != null)
                sw.WriteLine($"Total number of dot products: {dotProduct.TotalNumber}");
            sw.WriteLine();

            var total = _analisisResult.FirstOrDefault(a => a.Code == (int)ProfilerEvent.ForwardSolving);

            if (total != null)
                sw.WriteLine($"Total SOLVING time {total.TotalTime.TotalSeconds:0.00000}, {total.TotalNumber} times");
        }

        private void WriteModelInfo(StreamWriter sw, int nMpi, int nThreads)
        {
            sw.WriteLine($"DateTime: {DateTime.Now}");
            sw.WriteLine("model parameters:");
            sw.WriteLine($"nx = {_model.Nx}, ny = {_model.Ny}, nz = {_model.Nz}");
            sw.WriteLine($"Number of MPI processes = {nMpi}");
            sw.WriteLine($"Number of threads = {nThreads}");
        }

        private class SubEvents
        {
            public SubEvents(ProfilerEvent topEvent)
            {
                TopEvent = topEvent;
                Events = new SubEvents[0];
            }

            public SubEvents(ProfilerEvent topEvent, params SubEvents[] events)
            {
                TopEvent = topEvent;
                Events = events;
            }

            public SubEvents(ProfilerEvent topEvent, params ProfilerEvent[] events)
            {
                TopEvent = topEvent;
                Events = events.Select(ev => new SubEvents(ev, new SubEvents[0])).ToArray();
            }

            public ProfilerEvent TopEvent { get; }

            public SubEvents[] Events { get; }
        }

        private double WriteTopLevelInfo(StreamWriter sw, ProfilerEvent topEvent, params SubEvents[] events)
        {
            return WriteTopLevelInfo(sw, new SubEvents(topEvent, events));
        }

        private double WriteTopLevelInfo(StreamWriter sw, ProfilerEvent topEvent)
        {
            return WriteTopLevelInfo(sw, new SubEvents(topEvent));
        }

        private double WriteTopLevelInfo(StreamWriter sw, ProfilerEvent topEvent, params ProfilerEvent[] events)
        {
            var subEvents = events.Select(ev => new SubEvents(ev)).ToArray();

            return WriteTopLevelInfo(sw, new SubEvents(topEvent, subEvents));
        }

        private double WriteTopLevelInfo(StreamWriter sw, SubEvents subEvents)
        {
            var topEvent = subEvents.TopEvent;
            var topStat = _analisisResult.FirstOrDefault(a => a.Code == (int)topEvent);

            if (topStat == null)
                return 0;

            var totalPercent = PercentOfEvent(topStat, ProfilerEvent.ForwardSolving);

            sw.WriteLine();
            sw.WriteLine();
            sw.WriteLine($"{topEvent}:");
            sw.WriteLine($"\tTotal time: {topStat.TotalTime.TotalSeconds:0.0000}, {topStat.TotalNumber} times, \t\t\t\t\tpercent of SOLVING: {totalPercent:F1}%");
            sw.WriteLine();

            double percentSumm = 0;

            foreach (var profilerEvent in subEvents.Events)
                percentSumm += WriteIfExist(sw, topEvent, profilerEvent, 1);

            if (percentSumm != 0)
                sw.WriteLine("\t\t\t\t\t\t\tCovered: {0:F1}%", percentSumm);

            return totalPercent;
        }


        private double WriteInfoAboutTensorAtoA(StreamWriter sw)
        {
            double percent = 0;

            percent += WriteTopLevelInfo(sw, ProfilerEvent.GreenAtoATotal,
                   new SubEvents(ProfilerEvent.GreenScalarAtoA,
                                 new SubEvents(ProfilerEvent.GreenScalarAtoACalcCalc),
                                 new SubEvents(ProfilerEvent.GreenScalarAtoAUnion),
                                 new SubEvents(ProfilerEvent.GreenScalarAtoASegments),
                                 new SubEvents(ProfilerEvent.GreenScalarAtoACommunicate,
                                                ProfilerEvent.GreenScalarAtoATrans)),
                   new SubEvents(ProfilerEvent.GreenTensorAtoA,
                                 ProfilerEvent.GreenTensorAtoACalc,
                                 ProfilerEvent.GreenTensorAtoAFft));

            return percent;
        }


        private double WriteInfoAboutCustomFft(StreamWriter sw)
        {
            double percent = 0;

            percent += WriteTopLevelInfo(sw, ProfilerEvent.CustomFft,
                new SubEvents(ProfilerEvent.CustomFftInitialTranspose),
                new SubEvents(ProfilerEvent.CustomFftFourierY),
                new SubEvents(ProfilerEvent.CustomFftBlockTransposeYtoX),
                new SubEvents(ProfilerEvent.CustomFftDistributedTranspose),
                new SubEvents(ProfilerEvent.CustomFftFourierX),
                new SubEvents(ProfilerEvent.CustomFftBlockTransposeXtoY),
                new SubEvents(ProfilerEvent.CustomFftFinalTranspose));

            return percent;
        }

        private double WriteInfoAboutMisc(StreamWriter sw)
        {
            double percent = 0;

            percent += WriteTopLevelInfo(sw, ProfilerEvent.CalcChi0);
            percent += WriteTopLevelInfo(sw, ProfilerEvent.CalcJScattered);
            percent += WriteTopLevelInfo(sw, ProfilerEvent.CalcEScattered);
            percent += WriteTopLevelInfo(sw, ProfilerEvent.FftwPlanCalculation);

            return percent;
        }

        private double WriteInfoAboutCie(StreamWriter sw)
        {
            return WriteTopLevelInfo(sw, ProfilerEvent.SolveCie,
                new SubEvents(ProfilerEvent.CalcDotProduct),
                new SubEvents(ProfilerEvent.ApplyOperatorA,
                    ProfilerEvent.OperatorAApplyR,
                    ProfilerEvent.OperatorAPrepareForForwardFft,
                    ProfilerEvent.OperatorAForwardFft,
                    ProfilerEvent.OperatorAMultiplication,
                    ProfilerEvent.OperatorABackwardFft,
                    ProfilerEvent.OperatorAExtractAfterBackwardFft,
                    ProfilerEvent.OperatorAFinish));
        }

        private double WriteInfoObservations(StreamWriter sw)
        {
            return WriteTopLevelInfo(sw, ProfilerEvent.ObservationsFullCalculation);
        }

        private double PercentOfEvent(ProfilerStatistics subStat, ProfilerEvent mainEvent)
        {
            var mainStat = _analisisResult.First(a => a.Code == (int)mainEvent);

            return (subStat.TotalTime.TotalSeconds / mainStat.TotalTime.TotalSeconds) * 100;
        }

        private double WriteIfExist(StreamWriter sw, ProfilerEvent topEvent, SubEvents subEvents, int depth)
        {
            var profEvent = subEvents.TopEvent;
            var stat = _analisisResult.FirstOrDefault(a => a.Code == (int)profEvent);

            if (stat == null)
                return 0;

            var percent = PercentOfEvent(stat, topEvent);

            for (int i = 0; i < depth; i++)
                sw.Write("\t");

            sw.WriteLine($"{profEvent.ToString().PadRight(30)} {stat.TotalTime.TotalSeconds:0.00000}, {percent:F1} %, {stat.TotalNumber} times");

            if (subEvents.Events.Length != 0)
            {
                double subPercentSumm = 0;

                sw.WriteLine();

                foreach (var subEvent in subEvents.Events)
                    subPercentSumm += WriteIfExist(sw, subEvents.TopEvent, subEvent, depth + 1);

                if (subPercentSumm != 0)
                {
                    for (int i = 0; i < depth; i++)
                        sw.Write("\t");

                    sw.WriteLine("\t\t\t\t\t\t\tCovered: {0:F1}%", subPercentSumm);
                }

                sw.WriteLine();
            }

            return percent;
        }

        public static void SaveProfilingResultsToCommon(string fileName, CartesianModel model, ProfilerStatistics[] analisisResult, int numberOfMpi, int numberOfThreads, int nhank)
        {
            if (analisisResult == null)
                throw new ArgumentNullException();

            try
            {
                Func<string, int, string> padStr = (str, p) => str.PadRight(p);
                Func<string, string> pad = (str) => padStr(str, 15);
                Func<TimeSpan, string> toStr = (ts) => padStr(ts.TotalSeconds.ToString("0.00000"), 15);

                if (!File.Exists(fileName))
                {
                    File.AppendAllText(fileName, $"{pad("Date")}{padStr("Time", 12)}" +
                                                 $"{padStr("nx", 6)}{padStr("ny", 6)}{padStr("nz", 6)}" +
                                                 $"{padStr("nhank", 6)}" +
                                                 $"{padStr("mpi_size", 9)}{padStr("nthreads", 9)}" +
                                                 $"{pad("AtoA_Green")}{pad("polX")}{pad("polY")}" +
                                                 $"{pad("AtoO_Green")}{pad("AtoO_calc_X")}{pad("AtoO_calc_Y")}\n");
                }


                var atoa = GetTimeSpan(analisisResult, ProfilerEvent.GreenAtoATotal, 0);
                var polX = GetTimeSpan(analisisResult, ProfilerEvent.SolveCie, 0);
                var polY = GetTimeSpan(analisisResult, ProfilerEvent.SolveCie, 1);
                var atoo =  GetTotalTime(analisisResult, ProfilerEvent.AtoOGreenCalc);

                var atoc1 = GetTimeSpan(analisisResult, ProfilerEvent.AtoOFields, 0);
                var atoc2 = GetTimeSpan(analisisResult, ProfilerEvent.AtoOFields, 1);

                var now = DateTime.Now;

                var line = $"{pad(now.ToShortDateString())}{padStr(now.ToShortTimeString(), 12)}" +
                           $"{padStr(model.Nx.ToString(), 6)}{padStr(model.Ny.ToString(), 6)}{padStr(model.Nz.ToString(), 6)}" +
                           $"{padStr(nhank.ToString(), 6)}" +
                           $"{padStr(numberOfMpi.ToString(), 9)}{padStr(numberOfThreads.ToString(), 9)}" +
                           $"{toStr(atoa)}{toStr(polX)}{toStr(polY)}" +
                           $"{toStr(atoo)}{toStr(atoc1)}{toStr(atoc2)}\n";

                File.AppendAllText(fileName, line);
            }

            catch (Exception ex)
            {

                Console.WriteLine($"Scalability profiling export {ex.Message} {ex.StackTrace}");


                //   throw;
            }
        }

        private static TimeSpan GetTimeSpan(ProfilerStatistics[] analisisResult, ProfilerEvent profEvent, int index)
        {
            return analisisResult.FirstOrDefault(a => a.Code == (int)profEvent)?.Times[index] ?? TimeSpan.Zero;
        }

        private static TimeSpan GetTotalTime(ProfilerStatistics[] analisisResult, ProfilerEvent profEvent)
        {
            return analisisResult.FirstOrDefault(a => a.Code == (int)profEvent)?.TotalTime ?? TimeSpan.Zero;
        }
    }
}
