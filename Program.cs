using System;
using Extreme.Cartesian.FftW;
using Extreme.Cartesian.Project;
using Extreme.Parallel;
using Porvem.ModelCreaters;

namespace ExtremeMt
{
    static class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                RunDemoMode();
            }
            else
            {
                string projectFileName = args[0];

                var project = XProjectSerializer.Load(projectFileName);
                RunFullVersion(project);
            }
        }

        private static void RunDemoMode()
        {
            Console.WriteLine("I am extrEMe MT, please provide *.xproj file");


            //var proj = XProject.NewWithFrequencies(0.001)
            //    .WithObservations(new ObservationLevel(0, 0, 0))
            //    .WithResultsPath(@"Z:\temp\comm3d3_one_block");


            //proj.ForwardSettings.Residual = 0.01;

            //var model = Commemi3D3ModelCreater.CreateModelWithFirstLayerBlockOnly();
            //var converter = new NonMeshedToCartesianModelConverter(model);
            //var mesh = new MeshParameters(40, 80, 32);
            //var cartesianModel = converter.Convert(mesh);

            //using (var memoryProvider = new FftWMemoryProvider())
            //{
            //    using (var runner = new DistributedSolverRunner(proj, memoryProvider))
            //    {
            //        runner.Run(cartesianModel);
            //    }
            //}
        }

        private static void RunFullVersion(XProject project)
        {
            using (var mpi = Mpi.Init())
            using (var memoryProvider = new FftWMemoryProvider())
            {
                using (var runner = new ExtremeMtSolverRunner(project, memoryProvider, mpi))
                {
                    var model = GeneralUtils.LoadCartesianModel(project.ModelFile, mpi);
                    runner.Run(model);
                }
            }
        }
    }
}
