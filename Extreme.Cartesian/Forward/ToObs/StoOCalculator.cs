//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
﻿using System;
using Extreme.Cartesian.Core;
using Extreme.Cartesian.Green;
using Extreme.Cartesian.Green.Scalar;
using Extreme.Cartesian.Green.Tensor;
using Extreme.Cartesian.Green.Tensor.Impl;
using Extreme.Cartesian.Model;
using Extreme.Core;

namespace Extreme.Cartesian.Forward
{
    public class StoOCalculator : ToOCalculator
    {
        private const decimal SourceThickness = 100m;

        private readonly AtoOLevelGreenTensorCalculator _gtc;
        private readonly TensorPlan _templateTensorPlan;
        private readonly ScalarPlansCreater _plansCreater;

        public StoOCalculator(ForwardSolver solver) : base(solver)
        {
            _templateTensorPlan = DistibutedUtils.CreateTensorPlanAtoO(Mpi, Model, 1, 1);
            _gtc = new AtoOLevelGreenTensorCalculator(Logger, Model, MemoryProvider);
            _plansCreater = new ScalarPlansCreater(Model.LateralDimensions, HankelCoefficients.LoadN40(), Solver.Settings.NumberOfHankels);
        }

        public void CalculateAnomalyFieldE(SourceLayer layer, ObservationLevel level, AnomalyCurrent src, AnomalyCurrent dst)
        {
            using (var greenTensor = CalculateGreenTensorElectric(layer, level))
                CalculateAnomalyFieldE(src, dst, greenTensor);
        }

        public void CalculateAnomalyFieldH(SourceLayer layer, ObservationLevel level, AnomalyCurrent src, AnomalyCurrent dst)
        {
            using (var greenTensor = CalculateGreenTensorMagnetic(layer, level))
                CalculateAnomalyFieldH(src, dst, greenTensor);
        }
        private GreenTensor CalculateGreenTensorMagnetic(SourceLayer layer, ObservationLevel level)
        {
            var plan = _plansCreater.CreateForSourceToObservation(layer, level);
            var transceiver = TranceiverUtils.CreateSourceToObservations(SourceThickness, layer, level);
            var scalarCalculator = GreenScalarCalculator.NewAtoOMagneticCalculator(Logger, Model);
            plan.CalculateI5 = false;
            var scalars = scalarCalculator.Calculate(plan, transceiver);

            using (var segments = ScalarSegments.AllocateAndConvert(MemoryProvider, plan, scalars))
            {
                var tensorPlan = new TensorPlan(_templateTensorPlan, layer, level);
                _gtc.ResetKnots();
                var gt = _gtc.CalculateAtoOMagnetic(segments, MemoryLayoutOrder.AlongVertical, tensorPlan);

                PerformGreenTensorFft(gt);
                return gt;
            }
        }

        private GreenTensor CalculateGreenTensorElectric(SourceLayer layer, ObservationLevel level)
        {
            var plan = _plansCreater.CreateForSourceToObservation(layer, level);
            var transceiver = TranceiverUtils.CreateSourceToObservations(SourceThickness, layer, level);
            var scalarCalculator = GreenScalarCalculator.NewAtoOElectricCalculator(Logger, Model);
            var scalars = scalarCalculator.Calculate(plan, transceiver);

            using (var segments = ScalarSegments.AllocateAndConvert(MemoryProvider, plan, scalars))
            {
                var tensorPlan = new TensorPlan(_templateTensorPlan, layer, level);
                _gtc.ResetKnots();
                var gt = _gtc.CalculateAtoOElectric(segments, MemoryLayoutOrder.AlongVertical, tensorPlan);

                PerformGreenTensorFft(gt);
                return gt;
            }
        }


        private unsafe void PerformGreenTensorFft(GreenTensor gt)
        {
            foreach (var component in gt.GetAvailableComponents())
            {
                var ptr = gt[component].Ptr;
                long length = gt.Nx * gt.Ny;

                Copy(length, ptr, Pool.Plan1.Buffer1Ptr);
                Pool.ExecuteForward(Pool.Plan1);
                Copy(length, Pool.Plan1.Buffer1Ptr, ptr);
            }
        }
    }
}
