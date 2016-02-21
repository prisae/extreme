using System;
using Extreme.Cartesian.Core;
using Extreme.Cartesian.Green.Tensor;
using Extreme.Cartesian.Model;
using Extreme.Core;
using Extreme.Parallel;

namespace Extreme.Cartesian.Forward
{
    public static class DistibutedUtils
    {

        public static unsafe AnomalyCurrent GatherFromAllProcesses(ForwardSolver solver, AnomalyCurrent field)
        {
            if (!solver.IsParallel)
                return field;

            int size = 3 * field.Nx * field.Ny;

            if (solver.Mpi.IsMaster)
            {
                var result = AnomalyCurrent.AllocateNew(solver.MemoryProvider, solver.Model.Nx, solver.Model.Ny, 1);

                solver.Mpi.Gather(solver.Pool.RealModelPartCommunicator,
                    result.Ptr, field.Ptr, size, size);

                return result;
            }
            else
            {
                solver.Mpi.Gather(solver.Pool.RealModelPartCommunicator,
                 null, field.Ptr, 0, size);

                return null;
            }
        }

        public static TensorPlan CreateTensorPlanAtoO(Mpi mpi, OmegaModel model, int nTr, int nRc)
        {
            if (mpi != null && mpi.IsParallel)
            {
                var nxStart = mpi.CalcLocalNxStart(model.LateralDimensions);
                var nxTotalLength = mpi.CalcLocalNxLength(model.LateralDimensions);
                var nxCalcLength = nxTotalLength;

                if (nxStart >= model.Nx)
                    nxStart = -2 * model.Nx + nxStart;

                if (nxStart == -model.Nx)
                {
                    nxStart++;
                    nxCalcLength--;
                }

                return new TensorPlan(nxStart, nxCalcLength, nxTotalLength, nTr, nRc);
            }
            else
            {
                var nxStart = -model.Nx + 1;
                var nxTotalLength = 2 * model.Nx;
                var nxCalcLength = nxTotalLength - 1;

                return new TensorPlan(nxStart, nxCalcLength, nxTotalLength, nTr, nRc);
            }
        }

        public static void CheckNumberOfProcesses(this Mpi mpi, LateralDimensions lateral)
        {
            if ((2 * lateral.Nx) % mpi.Size != 0)
                throw new InvalidOperationException(
                    $"Number of MPI processes {mpi.Size} is not correct fo current Nx {lateral.Nx}");
        }

        public static int CalcLocalNxStart(this Mpi mpi, LateralDimensions lateral)
        {
            var fullNx = lateral.Nx * 2;

            if (fullNx % mpi.Size != 0)
                throw new ArgumentException($"fullNx % mpi.Size != 0, mpi.Size = [{mpi.Size}], fullNx = [{fullNx}]");

            var localNxSize = fullNx / mpi.Size;

            return localNxSize * mpi.Rank;
        }

        public static int CalcLocalNxLength(this Mpi mpi, LateralDimensions lateral)
            => CalcLocalNxLength(mpi, lateral.Nx*2);

        public static int CalcLocalNxLength(this Mpi mpi, int fullNx)
        {
            if (fullNx % mpi.Size != 0)
                throw new ArgumentException($"fullNx % mpi.Size != 0, mpi.Size = [{mpi.Size}], fullNx = [{fullNx}]");

            return fullNx / mpi.Size;
        }

        public static int CalcLocalHalfNxStart(this Mpi mpi, int rank, int nx)
        {
            var result = mpi.CalcHalfLocal(rank, nx);
            if (result.Item1 >= nx)
                return 0;

            return result.Item1;
        }

        public static int CalcLocalHalfNxLength(this Mpi mpi, int rank, int nx)
        {
            var result = mpi.CalcHalfLocal(rank, nx);
            if (result.Item1 >= nx)
                return 0;

            return result.Item2;
        }

        public static int CalcLocalHalfNxStart(this Mpi mpi, int nx)
            => CalcLocalHalfNxStart(mpi, mpi.Rank, nx);

        public static int CalcLocalHalfNxLength(this Mpi mpi, int nx)
            => CalcLocalHalfNxLength(mpi, mpi.Rank, nx);

        private static Tuple<int, int> CalcHalfLocal(this Mpi mpi, int rank, int nx)
        {
            if (mpi.Size == 1)
                return new Tuple<int, int>(0, nx);

            var halfNx = nx;
            var halfSize = (mpi.Size / 2);

            if (halfNx % halfSize != 0)
                throw new ArgumentException($"halfNx % halfSize != 0, halfSize = [{halfSize}], halfNx = [{halfNx}]");

            var halfNxLength = halfNx / halfSize;
            var halfNxStart = halfNxLength * rank;

            return new Tuple<int, int>(halfNxStart, halfNxLength);
        }

    }
}
