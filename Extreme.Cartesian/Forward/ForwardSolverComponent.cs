using Extreme.Cartesian.Fft;
using Extreme.Cartesian.Model;
using Extreme.Core;
using Extreme.Parallel;


namespace Extreme.Cartesian.Forward
{
    public class ForwardSolverComponent
    {
        protected ForwardSolver Solver { get; }
        protected OmegaModel Model => Solver.Model;
        protected INativeMemoryProvider MemoryProvider => Solver.MemoryProvider;
        protected ILogger Logger => Solver.Logger;
        protected IProfiler Profiler => Solver.Profiler;
        protected Mpi Mpi => Solver.Mpi;
        protected FftBuffer Pool => Solver.Pool;

        protected ForwardSolverComponent(ForwardSolver solver)
        {
            Solver = solver;
        }
    }
}
