using System;
using System.IO;
using System.Linq;
using Microsoft.Office.Interop.Excel;
using ModelCreaters;
using Porvem.Cartesian;
using Porvem.Cartesian.Forward.Magnetotellurics;
using Porvem.Cartesian.Model;
using Porvem.Cartesian.Project;
using Porvem.Core;

namespace Profiling
{
    public static class Program
    {
        private static string ProfilingResultsPath = @"Z:\PorvemWorking\Profiling\prof1.xml";

        static void Main(string[] args)
        {
            //RunModel();
            PerformProfilingResultsAnalize(ProfilingResultsPath);


            //var excel = new Application();

            //var wb = excel.Workbooks.Add();
            //excel.Visible = true;

            //var sheet = (Worksheet)wb.Sheets[1];

            //sheet.Name = "Profiling";



            //var chart = (Chart) sheet.Shapes.AddChart( );

            //chart.ChartTitle.Text = "Profiling chart";
        }


        private static void RunModel()
        {
            const double frequency = 0.1;
            var project = XProject.NewWithFrequencies(frequency);

            project.NumberOfHankels = 1;
            project.Residual = 1E-12;

            using (INativeMemoryProvider memoryProvider = new MarshalAllocHGlobalMemoryProvider())
            {
                foreach (var freq in project.Frequencies)
                {
                    var mesh = new MeshParameters(10, 10, 10);
                    var cartesianModel = SimpleCommemi3DModelCreater.CreateCartesianModel(1, 0.01, mesh);
                    var omegaModel = OmegaModelBuilder.BuildOmegaModel(cartesianModel, freq);

                    var logger = new ConsoleLogger();
                    var profiler = new Profiler();
                    var factory = new MtForwardSolverFactory(logger, profiler, memoryProvider, project, omegaModel);

                    var fieldReciepient = new FieldRecipientCollector();
                    var resultsContainer = new ResultsContainer(omegaModel.LateralDimensions);

                    var solver = factory.CreateNewForwardSolver(fieldReciepient);

                    solver.Solve();

                    ProfilerResultsTextExporter.SaveProfilingResults(ProfilingResultsPath, cartesianModel, profiler);

                    memoryProvider.ReleaseAllMemory();
                }
            }
        }


        private static void PerformProfilingResultsAnalize(string path)
        {
            var dir = Path.GetDirectoryName(path);

            var records = ProfilerResultsSerializer.LoadRecords(path);
            var model = ProfilerResultsSerializer.LoadModel(path);

            var analizer = new ProfilerStatisticsAnalyzer(records);
            var analisisResult = analizer.PerformAnalysis();

            ProfilerResultsTextExporter.SaveProfilingResultsTo(Path.Combine(dir, "profiler_result_type3_1.txt"), analisisResult, model);

            //var str1 = analizer.ConvertToString1(analisisResult);
            //var str2 = analizer.ConvertToString2(analisisResult);

            //var path1 = Path.Combine(dir, "profiler_result_type1.txt");
            //var path2 = Path.Combine(dir, "profiler_result_type2.txt");

            //File.WriteAllText(path1, str1);
            //File.WriteAllText(path2, str2);
        }
    }
}
