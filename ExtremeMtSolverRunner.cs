using System;
using System.IO;
using Extreme.Cartesian.Core;
using Extreme.Cartesian.Fft;
using Extreme.Cartesian.Forward;
using Extreme.Cartesian.Logger;
using Extreme.Cartesian.Model;
using Extreme.Core;
using Extreme.Core.Logger;
using Extreme.Parallel;
using Extreme.Parallel.Logger;
using Extreme.Cartesian.Magnetotellurics;
using Profiling;

namespace ExtremeMt
{

    public class ExtremeMtSolverRunner : IDisposable
    {
        private readonly ForwardProject _project;
        private readonly INativeMemoryProvider _memoryProvider;
        private readonly Mpi _mpi;
        private readonly ILogger _logger;
        private readonly Profiler _profiler = new Profiler();

        private readonly Mt3DForwardSolver _solver;

        public ExtremeMtSolverRunner(ForwardProject project, INativeMemoryProvider memoryProvider, Mpi mpi = null)
        {
            if (project == null) throw new ArgumentNullException(nameof(project));
            if (memoryProvider == null) throw new ArgumentNullException(nameof(memoryProvider));

            _project = project;
            _memoryProvider = memoryProvider;
            _mpi = mpi;
            _logger = (_mpi == null || !_mpi.IsParallel) ?
                (ILogger)new ConsoleLogger() :
                new ParallelConsoleLogger(_mpi);

            _solver = new Mt3DForwardSolver(_logger, memoryProvider, _project.ForwardSettings);
            _solver.SetProfiler(_profiler);
        }

        public void Run(CartesianModel model)
        {
            FftBuffersPool.PrepareBuffersForModel(model, _memoryProvider, _mpi, _profiler);

            LogSettingsInfo();

            _solver.SetProfiler(_profiler);
            _solver
                .WithMpi(_mpi)
                .With(_project.ObservationLevels);

            int freqCounter = 0;

            foreach (var frequency in _project.Frequencies)
            {
                ForwardLoggerHelper.WriteStatus(_logger, $"\t\t\tFrequecy {frequency}, {freqCounter++} of {_project.Frequencies.Count}");
                var omegaModel = OmegaModelBuilder.BuildOmegaModel(model, frequency);
                _profiler.ClearAllRecords();

                using (var rc = _solver.Solve(omegaModel))
                {
                    if (!_solver.IsParallel || _mpi.IsMaster)
                    {
                        ExportProfiling(model, frequency);
                        Export(rc, frequency);
                    }

                    ForwardLoggerHelper.WriteStatus(_logger, "Finish");
                    ParallelMemoryUtils.ExportMemoryUsage(_project.ResultsPath, _mpi, _memoryProvider, frequency);
                }
            }
        }

        private void LogSettingsInfo()
        {
            ForwardLoggerHelper.WriteStatus(_logger, $"Number of threads: {MultiThreadUtils.MaxDegreeOfParallelism}");
            ForwardLoggerHelper.WriteStatus(_logger, $"Number of hankels: {_project.ForwardSettings.NumberOfHankels}");
            ForwardLoggerHelper.WriteStatus(_logger, $"Target residual  : {_project.ForwardSettings.Residual}");
        }

        private void Export(ResultsContainer rc, double frequency)
        {
            ForwardLoggerHelper.WriteStatus(_logger, "Exporting results...");

            var resultPath = _project.ResultsPath;
            var responsesFileName = PlainTextExporter.GetResponsesFileName(frequency);
            var fieldsFileName = PlainTextExporter.GetFieldsFileName(frequency);

            if (!Directory.Exists(resultPath))
                Directory.CreateDirectory(resultPath);

            var exporter = new PlainTextExporter(rc, frequency);

            ForwardLoggerHelper.WriteStatus(_logger, "\t Export Raw fields");
            exporter.ExportRawFields(Path.Combine(resultPath, fieldsFileName));

            ForwardLoggerHelper.WriteStatus(_logger, "\t Export MT responses");
            exporter.ExportMtResponses(Path.Combine(resultPath, responsesFileName));
        }

        private void ExportProfiling(CartesianModel model, double frequency)
        {
            ForwardLoggerHelper.WriteStatus(_logger, "Exporting profiling results");
            var dir = _project.ResultsPath;
            var freqStr = $"_freq{frequency:####0.0000}";

            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            //ProfilerResultsTextExporter.SaveRawProfilingResults(Path.Combine(dir, $"rawprofile{freqStr}.xml"), model, _profiler);

            var records = _profiler.GetAllRecords();
            var analizer = new ProfilerStatisticsAnalyzer(records);
            var analisisResult = analizer.PerformAnalysis();


            int numberOfMpi = _mpi?.Size ?? 1;
            int numberOfThreads = MultiThreadUtils.MaxDegreeOfParallelism;

            var file = Path.Combine(dir, $"profiling_mpi{numberOfMpi:0000}_threads{numberOfThreads}{freqStr}.txt");

            ProfilerResultsTextExporter.SaveProfilingResultsTo(file, model, analisisResult, numberOfMpi, numberOfThreads);

            var file2 = Path.Combine(dir, "..", "common.dat");
            ProfilerResultsTextExporter.SaveProfilingResultsToCommon(file2, model, analisisResult, numberOfMpi, numberOfThreads, _project.ForwardSettings.NumberOfHankels);
        }

        public void Dispose()
        {
            _solver.Dispose();
        }
    }
}
