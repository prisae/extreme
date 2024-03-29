//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
﻿using System;
using Extreme.Cartesian.FftW;
using Extreme.Cartesian.Forward;
using Extreme.Model;
using Extreme.Parallel;

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

                var project = ForwardProjectSerializer.Load(projectFileName);
                RunFullVersion(project);
            }
        }

        private static void RunDemoMode()
        {
            Console.WriteLine("I am extrEMe MT, please provide *.xproj file");
        }

        private static void RunFullVersion(ForwardProject project)
        {
			using (var mpi_world = Mpi.Init ()) {
				var mpi = mpi_world.Dup ();
				using (var memoryProvider = new FftWMemoryProvider ()) {
					using (var runner = new ExtremeMtSolverRunner (project, memoryProvider, mpi)) {
						var model = ModelGenUtils.LoadCartesianModel (project.ModelFile, mpi);
						runner.Run (model);
					}
				}
				mpi.Dispose ();
			}
        }
    }
}
