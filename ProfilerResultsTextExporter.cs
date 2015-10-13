using System;
using System.IO;
using System.Linq;
using Porvem.Cartesian.Model;
using Porvem.Core;

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
                sw.WriteLine("Total number of multiplications: {0}", mult.TotalNumber);

            if (dotProduct != null)
                sw.WriteLine("Total number of dot products: {0}", dotProduct.TotalNumber);
            sw.WriteLine();

            var total = _analisisResult.FirstOrDefault(a => a.Code == (int)ProfilerEvent.ForwardSolving);

            if (total != null)
                sw.WriteLine("Total SOLVING time {0}, {1} times", total.TotalTime, total.TotalNumber);
        }

        private void WriteModelInfo(StreamWriter sw, int nMpi, int nThreads)
        {
            sw.WriteLine("DateTime: {0}", DateTime.Now);
            sw.WriteLine("model parameters:");
            sw.WriteLine("nx = {0}, ny = {1}, nz = {2}",
                _model.LateralDimensions.Nx,
                _model.LateralDimensions.Ny,
                _model.Anomaly.Layers.Count);
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
            sw.WriteLine("{0}:", topEvent);
            sw.WriteLine("\tTotal time: {0}, {2} times, \t\t\t\t\tpercent of SOLVING: {1:F1}%", topStat.TotalTime, totalPercent, topStat.TotalNumber);
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
                                 ProfilerEvent.GreenScalarAtoACalc,
                                 ProfilerEvent.GreenScalarAtoACommunicate),
                   new SubEvents(ProfilerEvent.GreenTensorAtoA,
                                 ProfilerEvent.GreenTensorAtoACalc,
                                 ProfilerEvent.GreenTensorAtoAFft));

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
            return WriteTopLevelInfo(sw, ProfilerEvent.ObservationsCalculation);
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

            sw.WriteLine("{2} {0}, {1:F1} %, {3} times", stat.TotalTime, percent, profEvent.ToString().PadRight(30), stat.TotalNumber);

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
    }
}
