using System;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Security;
using Extreme.Cartesian.Core;
using Extreme.Cartesian.Forward;
using Extreme.Cartesian.Logger;
using Extreme.Cartesian.Model;
using Extreme.Core;
using Extreme.Fgmres;
using Extreme.Parallel;

namespace Extreme.Cartesian.Convolution
{
    public unsafe sealed class AnomalyCurrentFgmresSolver : IDisposable
    {
        private const int Exit = 0;
        private const int Mult = 1;
        private readonly ForwardSolver _solver;
        
        private readonly FlexibleGmresWithGmresPreconditioner _fgmres;
        private readonly int _problemSize;

        private int _numberOfMults;
        private int _numberOfDotProducts;
        private ConvolutionOperator _operatorA;

        private OmegaModel Model => _solver.Model;

        public AnomalyCurrentFgmresSolver(ForwardSolver solver)
        {
            if (solver == null) throw new ArgumentNullException(nameof(solver));

            _solver = solver;
            _problemSize = Model.Anomaly.LocalSize.Nx * Model.Anomaly.LocalSize.Ny * Model.Nz * 3;

            var settings = _solver.Settings;

            var fgmresParams = new GmresParams(_problemSize, settings.OuterBufferLength)
            {
                Tolerance = settings.Residual,
                MaxRepeatNumber = settings.MaxRepeatsNumber,
                ResidualAtRestart = ResidualAtRestart.ComputeTheTrue,
                InitialGuess = InitialGuess.UserSupplied,
                GramSchmidtType = GramSchmidtType.IterativeClassical,
            };

            _fgmres = new FlexibleGmresWithGmresPreconditioner(_solver.Logger, _solver.MemoryProvider,
                fgmresParams, settings.InnerBufferLength);

            _fgmres.MatrixVectorMultRequest += _solver_MatrixVectorMultRequest;
            _fgmres.DotProductRequest += _solver_DotProductRequest;
            _fgmres.IterationComplete += solver_IterationComplete;
        }

        private bool IsSlavePart => _solver.Mpi.Rank >= _solver.Mpi.Size / 2;

        public void Solve(ConvolutionOperator operatorA, AnomalyCurrent b, AnomalyCurrent x)
        {
            if (operatorA == null) throw new ArgumentNullException(nameof(operatorA));
            if (b == null) throw new ArgumentNullException(nameof(b));
            if (x == null) throw new ArgumentNullException(nameof(x));

            _operatorA = operatorA;

            if (_solver.IsParallel && IsSlavePart)
                RunSlavePart(b, x);
            else
                RunMainPart(b, x);
        }

        private void RunMainPart(AnomalyCurrent b, AnomalyCurrent x)
        {
            var rhs = new NativeVector(b.Ptr, _problemSize);
            var result = new NativeVector(x.Ptr, _problemSize);

            _numberOfMults = 0;
            _numberOfDotProducts = 0;

            _fgmres.Solve(rhs, rhs, result);

            if (_solver.IsParallel)
                SendCommand(Exit);
        }

        private void RunSlavePart(AnomalyCurrent b, AnomalyCurrent x)
        {
            while (RecvCommandFromMaster() != Exit)
                _operatorA.Apply(b, x);
        }

        private int RecvCommandFromMaster()
            => _solver.Mpi.BroadCast(Mpi.CommWorld, Mpi.Master, 0);

        private void SendCommand(int command)
            => _solver.Mpi.BroadCast(Mpi.CommWorld, Mpi.Master, command);

        private void _solver_MatrixVectorMultRequest(object sender, MatrixVectorMultRequestEventArgs e)
        {
            using (_solver.Profiler?.StartAuto(ProfilerEvent.ApplyOperatorA))
            {
                if (_solver.IsParallel)
                    SendCommand(Mult);

                var ls = Model.Anomaly.LocalSize;
                var inp = AnomalyCurrent.ReUseMemory(e.X.Ptr, ls.Nx, ls.Ny, Model.Nz);
                var res = AnomalyCurrent.ReUseMemory(e.Result.Ptr, ls.Nx, ls.Ny, Model.Nz);

                _operatorA.Apply(inp, res);
                _numberOfMults++;
            }
        }

        private void _solver_DotProductRequest(object sender, DotProductRequestEventArgs e)
        {
            using (_solver.Profiler?.StartAuto(ProfilerEvent.CalcDotProduct))
            {
                for (int k = 0; k < e.NumberOfDotProducts; k++)
                    CalculateProductConjugatedLeft(e.Length, e.X + e.Length * k, e.Y, e.Result + k);

                _numberOfDotProducts += e.NumberOfDotProducts;

                if (_solver.IsParallel)
                {
                    for (int i = 0; i < e.NumberOfDotProducts; i++)
                        e.Result[i] = _solver.Mpi.AllReduce(_solver.Pool.RealModelPartCommunicator, e.Result[i]);
                }
            }
        }

        private void solver_IterationComplete(object sender, GmresIterationCompleteEventArgs e)
        {
            _solver.Logger.WriteStatus($@"iteration: {e.NumberOfIteration}, residual: {e.ArnoldiBackwardError:E5}");
            var message =
                $"Total multiplications: {_numberOfMults}, Total dot products: {_numberOfDotProducts}";

            _solver.Logger.WriteStatus(message);
        }

        private const string LibNative = @"ntv_math";

        [SuppressUnmanagedCodeSecurity]
        [DllImport(LibNative, EntryPoint = "CalculateDotProductConjugatedLeft")]
        private static extern void CalculateProductConjugatedLeft(long size, Complex* m1, Complex* m2, Complex* result);

        public void Dispose()
        {
            _fgmres.Dispose();
        }
    }
}