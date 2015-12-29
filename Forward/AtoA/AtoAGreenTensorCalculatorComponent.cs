using System;
using System.Linq;
using System.Numerics;
using Extreme.Cartesian.Green;
using Extreme.Cartesian.Green.Scalar;
using Extreme.Cartesian.Green.Tensor;
using Extreme.Cartesian.Green.Tensor.Impl;
using Extreme.Cartesian.Model;
using Extreme.Core;
using Extreme.Parallel;

namespace Extreme.Cartesian.Forward
{
    public unsafe class AtoAGreenTensorCalculatorComponent : ForwardSolverComponent
    {
        private readonly ScalarPlan _plan;
        private readonly GreenScalarCalculator _scalarCalc;
        private readonly AtoAGreenTensorCalculator _tensorCalc;

        private readonly int _realCalcNxStart;
        private readonly int _totalNxLength;
        private readonly int _calcNxLength;
        private readonly bool _mirroringX;

        public AtoAGreenTensorCalculatorComponent(ForwardSolver solver) : base(solver)
        {
            _plan = new ScalarPlansCreater(Model.LateralDimensions, HankelCoefficients.LoadN40(), Solver.Settings.NumberOfHankels)
                        .CreateForAnomalyToAnomaly();

            _scalarCalc = GreenScalarCalculator.NewAtoACalculator(Logger, Model);
            _tensorCalc = new AtoAGreenTensorCalculator(Logger, Model, MemoryProvider);

            if (Solver.IsParallel)
            {
                var localNxStart = Mpi.CalcLocalNxStart(Model.LateralDimensions);
                _totalNxLength = Mpi.CalcLocalNxLength(Model.LateralDimensions);

                _mirroringX = localNxStart >= Model.Nx;
                _realCalcNxStart = _mirroringX ? 2 * Model.Nx - localNxStart - _totalNxLength + 1 : localNxStart;

                _calcNxLength = _totalNxLength;
                if (_realCalcNxStart + _totalNxLength == Model.Nx + 1)
                    _calcNxLength--;
            }
            else
            {
                _mirroringX = false;
                _realCalcNxStart = 0;
                _totalNxLength = 2 * Model.Nx;
                _calcNxLength = Model.Nx;
            }
        }

        public GreenTensor CalculateGreenTensor()
        {
            using (Profiler?.StartAuto(ProfilerEvent.GreenAtoATotal))
            {
                _tensorCalc.SetNxSizes(_realCalcNxStart, _totalNxLength, _calcNxLength);

                var asym = CalculateAsymGreenTensorAtoA();
                var symm = CalculateSymmGreenTensorAtoA();
                var tensor = GreenTensor.Merge(asym, symm);

                return tensor;
            }
        }



        private GreenTensor CalculateAsymGreenTensorAtoA()
        {
            _plan.SetOnlyAsym();
            var trcAsym = TranceiverUtils.CreateAnomalyToAnomaly(Model.Anomaly);

            MemoryUtils.PrintMemoryReport("Before scalars ASYM", Logger, MemoryProvider);
            var scalarsAsym = CalculateScalars(_plan, trcAsym.ToArray());
            
            MemoryUtils.PrintMemoryReport("After scalars ASYM", Logger, MemoryProvider);

            using (Profiler?.StartAuto(ProfilerEvent.GreenTensorAtoA))
            {
                GreenTensor tensor;

                using (Profiler?.StartAuto(ProfilerEvent.GreenTensorAtoACalc))
                {
                    MemoryUtils.PrintMemoryReport("Before tensors ASYM", Logger, MemoryProvider);
                    tensor = _tensorCalc.CalculateAsymAtoA(scalarsAsym, MemoryLayoutOrder.AlongVertical, _mirroringX);
                    MemoryUtils.PrintMemoryReport("after tensor ASYM", Logger, MemoryProvider);
                    scalarsAsym.Dispose();
                    MemoryUtils.PrintMemoryReport("after SCAl Dispose ASYM", Logger, MemoryProvider);
                }

                PopulateForFft(tensor, MemoryLayoutOrder.AlongVertical, _totalNxLength);
                if (_mirroringX) AddMinusToXz(tensor);

                using (Profiler?.StartAuto(ProfilerEvent.GreenTensorAtoAFft))
                {
                    PerformFftAsym(tensor, MemoryLayoutOrder.AlongVertical);
                }

                return tensor;
            }
        }

        private GreenTensor CalculateSymmGreenTensorAtoA()
        {
            _plan.SetOnlySymm();
            var trcSymm = TranceiverUtils.CreateAnomalyToAnomalySymm(Model.Anomaly);
            MemoryUtils.PrintMemoryReport("Before scalars SYM", Logger, MemoryProvider);
            var scalarsSymm = CalculateScalars(_plan, trcSymm.ToArray());
            MemoryUtils.PrintMemoryReport("After scalars SYM", Logger, MemoryProvider);
            
            using (Profiler?.StartAuto(ProfilerEvent.GreenTensorAtoA))
            {
                GreenTensor tensor;
                using (Profiler?.StartAuto(ProfilerEvent.GreenTensorAtoACalc))
                {
                    MemoryUtils.PrintMemoryReport("Before tensors SYM", Logger, MemoryProvider);
                    tensor = _tensorCalc.CalculateSymmAtoA(scalarsSymm, MemoryLayoutOrder.AlongVertical, _mirroringX);
                    MemoryUtils.PrintMemoryReport("After tensors SYM", Logger, MemoryProvider);
                    scalarsSymm.Dispose();
                    MemoryUtils.PrintMemoryReport("after SCAl Dispose SYM", Logger, MemoryProvider);
                }

                PopulateForFft(tensor, MemoryLayoutOrder.AlongVertical, _totalNxLength, true);

                if (_mirroringX)
                    AddMinusToXy(tensor);

                using (Profiler?.StartAuto(ProfilerEvent.GreenTensorAtoAFft))
                {
                    PerformFftSymm(tensor, MemoryLayoutOrder.AlongVertical);
                }

                return tensor;
            }
        }

        #region Fft


        private void PopulateForFft(GreenTensor gt, MemoryLayoutOrder layoutOrder, int nxLength, bool symm = false)
        {
            if (Solver.IsParallel)
                new GreenTensorFftPopulator(gt, Model, symm)
                    .PopulateForFftDistributedAlongX(layoutOrder, nxLength);
            else
                new GreenTensorFftPopulator(gt, Model, symm)
                    .PopulateForFft(layoutOrder);
        }

        private void PerformFftAsym(GreenTensor gt, MemoryLayoutOrder layoutOrder)
        {
            if (layoutOrder != MemoryLayoutOrder.AlongVertical)
                throw new InvalidOperationException($"{nameof(layoutOrder)} should be AlongVertical to perform fft");

            var nz = Model.Nz;
            int sizeAsym = nz * nz;

            MakeFftForComponent(gt, "xz", sizeAsym);
            MakeFftForComponent(gt, "yz", sizeAsym);
        }

        private void PerformFftSymm(GreenTensor gt, MemoryLayoutOrder layoutOrder)
        {
            if (layoutOrder != MemoryLayoutOrder.AlongVertical)
                throw new InvalidOperationException($"{nameof(layoutOrder)} should be AlongVertical to perform fft");

            var nz = Model.Nz;
            int sizeSymm = (nz * (nz + 1)) / 2;

            MakeFftForComponent(gt, "xx", sizeSymm);
            MakeFftForComponent(gt, "xy", sizeSymm);
            MakeFftForComponent(gt, "yy", sizeSymm);
            MakeFftForComponent(gt, "zz", sizeSymm);
        }

        private void MakeFftForComponent(GreenTensor gt, string comp, int size)
        {
            var ptr = gt[comp].Ptr;
            var nz3 = 3 * Model.Nz;

            int layerSize = gt.Nx * gt.Ny;

            for (int i = 0; i < size; i += nz3)
            {
                int length = i + nz3 <= size ? nz3 : size - i;

                CopyToBuffer(i, length, layerSize, size, ptr);
                Pool.ExecuteForward(Pool.Plan3Nz);
                CopyFromBuffer(i, length, layerSize, size, ptr);
            }
        }

        private void CopyToBuffer(int start, int length, int layerSize, int nzSize, Complex* ptr)
        {
            var nz3 = 3 * Model.Nz;
            var dst = Pool.Plan3Nz.Buffer1Ptr;

            for (int i = 0; i < layerSize; i++)
            {
                long srcShift = nzSize * i + start;
                long dstShift = i * nz3;

                for (int k = 0; k < length; k++)
                    dst[dstShift + k] = ptr[srcShift + k];

            }
        }

        private void CopyFromBuffer(int start, int length, int layerSize, int nzSize, Complex* ptr)
        {
            var nz3 = 3 * Model.Nz;
            var src = Pool.Plan3Nz.Buffer1Ptr;

            for (int i = 0; i < layerSize; i++)
            {
                long dstShift = nzSize * i + start;
                long srcShift = i * nz3;
		
                for (int k = 0; k <  length; k++)
                    ptr[dstShift + k] = src[srcShift + k];
            }
        }

        private void AddMinusToXz(GreenTensor tensor)
        {
            var ptr = tensor["xz"].Ptr;
            int length = tensor.Ny * tensor.Nx * Model.Nz * Model.Nz;
            for (long i = 0; i < length; i++)
                ptr[i] = -ptr[i];
        }

        private void AddMinusToXy(GreenTensor tensor)
        {
            var ptr = tensor["xy"].Ptr;
            var nz = Model.Nz;
            int length = tensor.Ny * tensor.Nx * (nz + nz * (nz - 1) / 2);

            for (long i = 0; i < length; i++)
                ptr[i] = -ptr[i];
        }
        #endregion

        #region Scalars

        private ScalarSegments CalculateScalars(ScalarPlan plan, Transceiver[] transceivers)
        {
            using (Profiler?.StartAuto(ProfilerEvent.GreenScalarAtoA))
            {
                if (Solver.IsParallel)
                    return CalculateParallelScalars(plan, transceivers);
                else
                    return CalculateLocalScalars(plan, transceivers);
            }
        }

        private ScalarSegments CalculateLocalScalars(ScalarPlan plan, Transceiver[] transceivers)
        {
            GreenScalars gs;

            using (Profiler?.StartAuto(ProfilerEvent.GreenScalarAtoACalc))
            {
                gs = _scalarCalc.Calculate(plan, transceivers, Profiler);
            }

            using (Profiler?.StartAuto(ProfilerEvent.GreenScalarAtoASegments))
            {
                var segments = ScalarSegments.AllocateAndConvert(MemoryProvider, plan, gs);
                return segments;
            }
        }

        private ScalarSegments CalculateParallelScalars(ScalarPlan plan, Transceiver[] transceivers)
        {
            var pm = new ParallelManager<Transceiver>(Mpi).WithTasks(transceivers);

            GreenScalars gs = null;
            ScalarSegments localSegments = null;

            pm.Run(tasks =>
            {
                using (Profiler?.StartAuto(ProfilerEvent.GreenScalarAtoACalc))
                {
                    gs = _scalarCalc.Calculate(plan, tasks, Profiler);
                }

                using (Profiler?.StartAuto(ProfilerEvent.GreenScalarAtoASegments))
                {
                    localSegments = ScalarSegments.AllocateAndConvert(MemoryProvider, plan, gs);
                }
            });

            using (Profiler?.StartAuto(ProfilerEvent.GreenScalarAtoACommunicate))
            {
                var allSegments1 = ScalarSegments.AllocateScalarSegments(MemoryProvider, plan, gs, transceivers.Length);
                var allSegments2 = ScalarSegments.AllocateScalarSegments(MemoryProvider, plan, gs, transceivers.Length);

                DistributeSegments(plan, pm, gs, localSegments, allSegments1, allSegments2);
                localSegments.Dispose();

                allSegments1.Dispose();
                return allSegments2;
            }
        }


        private void DistributeSegments(ScalarPlan plan, ParallelManager<Transceiver> pm, GreenScalars gs, ScalarSegments local, ScalarSegments all, ScalarSegments result)
        {
            var starts = pm.GetAllStartIndecies();
            var lengths = pm.GetAllLength();
            var fullStarts = new int[lengths.Length];
            var fullLength = new int[lengths.Length];

            var localLength = gs.SingleScalars.Length;

            int nComp = gs.GetNumberOfAvailableIs(plan);
            int nRho = local.Radii.Length - 1;

            var localSize = localLength * nComp * 2 * nRho;

            for (int i = 0; i < starts.Length; i++)
            {
                fullStarts[i] = starts[i] * nComp * 2 * nRho;
                fullLength[i] = lengths[i] * nComp * 2 * nRho;
            }

            Mpi.AllGatherV(local.Ptr, localSize, all.Ptr, fullLength, fullStarts);

            using (Profiler?.StartAuto(ProfilerEvent.GreenScalarAtoATrans))
            {
                for (int i = 0; i < starts.Length; i++)
                {
                    int start = starts[i];
                    int length = lengths[i];

                    var tmp = ScalarSegments.ReUseScalarSegments(all.Ptr + fullStarts[i], plan, gs, length);

                    for (int j = 0; j < result.SingleSegment.Length; j++)
                    {
                        var src = tmp.SingleSegment[j];
                        var dst = result.SingleSegment[j];

                        if (src.I1A != null)
                            for (int k = 0; k < length; k++)
                                dst.I1A[k + start] = src.I1A[k];
                        if (src.I2A != null)
                            for (int k = 0; k < length; k++)
                                dst.I2A[k + start] = src.I2A[k];
                        if (src.I3A != null)
                            for (int k = 0; k < length; k++)
                                dst.I3A[k + start] = src.I3A[k];
                        if (src.I4A != null)
                            for (int k = 0; k < length; k++)
                                dst.I4A[k + start] = src.I4A[k];
                        if (src.I5A != null)
                            for (int k = 0; k < length; k++)
                                dst.I5A[k + start] = src.I5A[k];

                        if (src.I1B != null)
                            for (int k = 0; k < length; k++)
                                dst.I1B[k + start] = src.I1B[k];
                        if (src.I2B != null)
                            for (int k = 0; k < length; k++)
                                dst.I2B[k + start] = src.I2B[k];
                        if (src.I3B != null)
                            for (int k = 0; k < length; k++)
                                dst.I3B[k + start] = src.I3B[k];
                        if (src.I4B != null)
                            for (int k = 0; k < length; k++)
                                dst.I4B[k + start] = src.I4B[k];
                        if (src.I5B != null)
                            for (int k = 0; k < length; k++)
                                dst.I5B[k + start] = src.I5B[k];
                    }
                }
            }
        }

        #endregion
    }
}
