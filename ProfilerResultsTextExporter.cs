using System;
using System.IO;
using System.Linq;
using Porvem.Cartesian.Model;
using Porvem.Core;

namespace Profiling
{
    public static class ProfilerResultsTextExporter
    {
        public static void SaveProfilingResults(string path, CartesianModel cartesianModel, Profiler profiler)
        {
            profiler.GetAllRecords().SaveWithModel(path, cartesianModel);
        }

        public static void SaveProfilingResultsTo(string path, ProfilerStatistics[] analisisResult, CartesianModel model)
        {
            SaveProfilingResultsTo(path, analisisResult, model.LateralDimensions.Nx, model.LateralDimensions.Ny,
                model.Anomaly.Layers.Count);
        }


        public static void SaveProfilingResultsTo(string path, ProfilerStatistics[] analisisResult, int nx, int ny, int nz)
        {
            double percentSumm = 0;

            using (var sw = new StreamWriter(path))
            {
                sw.WriteLine("DateTime: {0}", DateTime.Now);
                sw.WriteLine("model parameters:");
                sw.WriteLine("nx = {0}, ny = {1}, nz = {2}", nx, ny, nz);

                percentSumm += WriteInfoAboutScalarAtoA(sw, analisisResult);
                percentSumm += WriteInfoAboutTensorAtoA(sw, analisisResult);
                percentSumm += WriteInfoAboutCie(sw, analisisResult);

                sw.WriteLine("\n\n                                               Total Covered: {0:F1} %", percentSumm);
            }
        }

        private static double WriteInfoAboutCie(StreamWriter sw, ProfilerStatistics[] analisisResult)
        {
            sw.WriteLine();
            sw.WriteLine("CIE:");

            var total = analisisResult.First(a => a.Code == (int)ProfilerEvent.ApplyOperatorA);

            double percentSumm = 0;
            Func<ProfilerStatistics, double> calcPercent =
                (ps =>
                {
                    var percent = (ps.TotalTime.TotalSeconds / total.TotalTime.TotalSeconds) * 100;
                    percentSumm += percent;
                    return percent;
                });

            var totalPercent = PercentOfForwardSolving(analisisResult, total);

            //var prepare = analisisResult.First(a => a.Code == (int)ProfilerEvent.OperatorAPrepare);
            //var applyR = analisisResult.First(a => a.Code == (int)ProfilerEvent.OperatorAApplyR);

            var fftF = analisisResult.FirstOrDefault(a => a.Code == (int)ProfilerEvent.OperatorAForwardFft);
            var fftB = analisisResult.FirstOrDefault(a => a.Code == (int)ProfilerEvent.OperatorABackwardFft);
            //var mult = analisisResult.First(a => a.Code == (int)ProfilerEvent.OperatorAMultiplicationLowLevel);

            //var finish = analisisResult.First(a => a.Code == (int)ProfilerEvent.OperatorAFinish);
            //var clear = analisisResult.First(a => a.Code == (int)ProfilerEvent.OperatorAClearBuffer1);

            WritePercentOfSolving(sw, total, totalPercent);

            sw.WriteLine();
            //sw.WriteStatus("\tapply R:         {0}, {1:F1} %", applyR.TotalTime, calcPercent(applyR));

            if (fftF != null)
                sw.WriteLine("\tforward fft:     {0}, {1:F1} %", fftF.TotalTime, calcPercent(fftF));

            if (fftB != null)
                sw.WriteLine("\tbackward fft:     {0}, {1:F1} %", fftB.TotalTime, calcPercent(fftB));
            

            //sw.WriteStatus("\tprepare:         {0}, {1:F1} %", prepare.TotalTime, calcPercent(prepare));

            //sw.WriteStatus("\tmult LL:         {0}, {1:F1} %", mult.TotalTime, calcPercent(mult));

            //sw.WriteStatus("\tbackward fft:    {0}, {1:F1} %", fftB.TotalTime, calcPercent(fftB));
            //sw.WriteStatus("\tclear buffer:    {0}, {1:F1} %", clear.TotalTime, calcPercent(clear));
            //sw.WriteStatus("\tfinish:          {0}, {1:F1} %", finish.TotalTime, calcPercent(finish));

            //sw.WriteStatus("\t                                Covered {0:F1} %", percentSumm);

            return totalPercent;
        }

        private static double PercentOfForwardSolving(ProfilerStatistics[] analisisResult, ProfilerStatistics item)
        {
            var totalSolving = analisisResult.First(a => a.Code == (int)ProfilerEvent.ForwardSolving);

            return (item.TotalTime.TotalSeconds / totalSolving.TotalTime.TotalSeconds) * 100;
        }

        private static double WriteInfoAboutScalarAtoA(StreamWriter sw, ProfilerStatistics[] analisisResult)
        {
            var scalarAtoA = analisisResult.First(a => a.Code == (int)ProfilerEvent.GreenScalarAtoA);
            var totalPercent = PercentOfForwardSolving(analisisResult, scalarAtoA);

            sw.WriteLine();
            sw.WriteLine("Scalar A to A:");
            WritePercentOfSolving(sw, scalarAtoA, totalPercent);

            return totalPercent;
        }

        private static void WritePercentOfSolving(StreamWriter sw, ProfilerStatistics scalarAtoA, double totalPercent)
        {
            sw.WriteLine("\ttotal time:      {0},               {1:F1}  %", scalarAtoA.TotalTime, totalPercent);
        }

        private static double WriteInfoAboutTensorAtoA(StreamWriter sw, ProfilerStatistics[] analisisResult)
        {
            sw.WriteLine();
            sw.WriteLine("Tensor:");

            var tensorFull = analisisResult.First(a => a.Code == (int)ProfilerEvent.GreenTensorAtoA);

            double percentSumm = 0;
            Func<ProfilerStatistics, double> calcPercent =
                (ps =>
                {
                    var percent = (ps.TotalTime.TotalSeconds / tensorFull.TotalTime.TotalSeconds) * 100;
                    percentSumm += percent;
                    return percent;
                });

            var totalPercent = PercentOfForwardSolving(analisisResult, tensorFull);

            //var tensorCalc = analisisResult.First(a => a.Code == (int)ProfilerEvent.GreenTensorAtoACalc);
            //var tensorFft = analisisResult.First(a => a.Code == (int)ProfilerEvent.GreenTensorAtoAFft);
            //var tensorPopulate = analisisResult.First(a => a.Code == (int)ProfilerEvent.GreenTensorAtoAPopulate);

            WritePercentOfSolving(sw, tensorFull, totalPercent);

            //sw.WriteStatus("\tcalc:            {0}, {1:F1} %", tensorCalc.TotalTime, calcPercent(tensorCalc));
            //sw.WriteStatus("\tfft:             {0}, {1:F1} %", tensorFft.TotalTime, calcPercent(tensorFft));
            //sw.WriteStatus("\tpopulate:        {0}, {1:F1} %", tensorPopulate.TotalTime, calcPercent(tensorPopulate));

            //sw.WriteStatus("\t                                Covered {0:F1} %", percentSumm);

            return totalPercent;
        }
    }
}
