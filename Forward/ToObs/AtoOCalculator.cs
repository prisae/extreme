using System;
using System.Collections.Generic;
using System.Numerics;
using Extreme.Cartesian.Core;
using Extreme.Cartesian.Green;
using Extreme.Cartesian.Green.Scalar;
using Extreme.Cartesian.Green.Tensor;
using Extreme.Cartesian.Green.Tensor.Impl;
using Extreme.Cartesian.Model;
using Extreme.Core;

namespace Extreme.Cartesian.Forward
{
    public class AtoOCalculator : ToOCalculator, IDisposable
    {
        private readonly AtoOLevelGreenTensorCalculator _gtc;
        private readonly TensorPlan _templateTensorPlan;
        private readonly ScalarPlansCreater _plansCreater;

        private readonly Dictionary<ObservationLevel, GreenTensor> _eGreenTensors = new Dictionary<ObservationLevel, GreenTensor>();
        private readonly Dictionary<ObservationLevel, GreenTensor> _hGreenTensors = new Dictionary<ObservationLevel, GreenTensor>();

        public AtoOCalculator(ForwardSolver solver) : base(solver)
        {
            _templateTensorPlan = DistibutedUtils.CreateTensorPlanAtoO(Mpi, Model, Model.Nz, 1);
            _gtc = new AtoOLevelGreenTensorCalculator(Logger, Model, MemoryProvider);
            _plansCreater = new ScalarPlansCreater(Model.LateralDimensions, HankelCoefficients.LoadN40(), Solver.Settings.NumberOfHankels);
        }

        #region Level

        public event EventHandler<GreenTensorCalculatedEventArgs> GreenTensorECalculated;
        public event EventHandler<GreenTensorCalculatedEventArgs> GreenTensorHCalculated;

        private void OnGreenTensorECalculated(GreenTensorCalculatedEventArgs e)
            => GreenTensorECalculated?.Invoke(this, e);

        private void OnGreenTensorHCalculated(GreenTensorCalculatedEventArgs e)
            => GreenTensorHCalculated?.Invoke(this, e);

        public void CalculateAnomalyFieldE(ObservationLevel level, AnomalyCurrent jQ, AnomalyCurrent field)
        {
            if (_eGreenTensors.ContainsKey(level))
            {
                using (Solver.Profiler?.StartAuto(ProfilerEvent.AtoOFields))
                {
                    CalculateAnomalyFieldE(jQ, field, _eGreenTensors[level]);
                }
                
                return;
            }

            var greenTensor = CalculateGreenTensorAtoOLevelElectric(level);
            var arg = new GreenTensorCalculatedEventArgs(level, greenTensor);
            OnGreenTensorECalculated(arg);

            using (Solver.Profiler?.StartAuto(ProfilerEvent.AtoOFields))
            {
                CalculateAnomalyFieldE(jQ, field, greenTensor);
            }

            if (arg.SupressGreenTensorDisposal)
                _eGreenTensors.Add(level, greenTensor);
            else
                greenTensor.Dispose();
        }

        public void CalculateAnomalyFieldH(ObservationLevel level, AnomalyCurrent jQ, AnomalyCurrent field)
        {
            if (_hGreenTensors.ContainsKey(level))
            {
                using (Solver.Profiler?.StartAuto(ProfilerEvent.AtoOFields))
                {
                    CalculateAnomalyFieldH(jQ, field, _hGreenTensors[level]);
                }
                return;
            }

            var greenTensor = CalculateGreenTensorAtoOLevelMagnetic(level);
            var arg = new GreenTensorCalculatedEventArgs(level, greenTensor);
            OnGreenTensorHCalculated(arg);

            using (Solver.Profiler?.StartAuto(ProfilerEvent.AtoOFields))
            {
                CalculateAnomalyFieldH(jQ, field, greenTensor);
            }

            if (arg.SupressGreenTensorDisposal)
                _hGreenTensors.Add(level, greenTensor);
            else
                greenTensor.Dispose();
        }

        private unsafe GreenTensor CalculateGreenTensorAtoOLevelMagnetic(ObservationLevel level)
        {
            using (Solver.Profiler?.StartAuto(ProfilerEvent.AtoOGreenCalc))
            {
                var plan = _plansCreater.CreateForAnomalyToObservationLevels(level);
                var transceiver = TranceiverUtils.CreateAnomalyToObservation(Model.Anomaly, level);
                var scalarCalculator = GreenScalarCalculator.NewAtoOMagneticCalculator(Logger, Model);
                plan.CalculateI5 = false;
                var scalars = scalarCalculator.Calculate(plan, transceiver);

                using (var segments = ScalarSegments.AllocateAndConvert(MemoryProvider, plan, scalars))
                {
                    var tensorPlan = new TensorPlan(_templateTensorPlan, level);
                    _gtc.ResetKnots();
                    var gt = _gtc.CalculateAtoOMagnetic(segments, MemoryLayoutOrder.AlongVertical, tensorPlan);

                    PerformGreenTensorFft(gt);
                    return gt;
                }
            }
        }

        private unsafe GreenTensor CalculateGreenTensorAtoOLevelElectric(ObservationLevel level)
        {
            using (Solver.Profiler?.StartAuto(ProfilerEvent.AtoOGreenCalc))
            {
                var plan = _plansCreater.CreateForAnomalyToObservationLevels(level);
                var transceiver = TranceiverUtils.CreateAnomalyToObservation(Model.Anomaly, level);
                var scalarCalculator = GreenScalarCalculator.NewAtoOElectricCalculator(Logger, Model);
                var scalars = scalarCalculator.Calculate(plan, transceiver);

                using (var segments = ScalarSegments.AllocateAndConvert(MemoryProvider, plan, scalars))
                {
                    var tensorPlan = new TensorPlan(_templateTensorPlan, level);
                    _gtc.ResetKnots();
                    var gt = _gtc.CalculateAtoOElectric(segments, MemoryLayoutOrder.AlongVertical, tensorPlan);

                    PerformGreenTensorFft(gt);
                    return gt;
                }
            }
        }

        private unsafe void PerformGreenTensorFft(GreenTensor gt)
        {
            foreach (var component in gt.GetAvailableComponents())
            {
                var ptr = gt[component].Ptr;
                long length = gt.Nx * gt.Ny * gt.NTr * gt.NRc;
                Copy(length, ptr, Pool.Plan1Nz.Buffer1Ptr);
                Pool.ExecuteForward(Pool.Plan1Nz);
                Copy(length, Pool.Plan1Nz.Buffer1Ptr, ptr);
            }
        }

        #endregion

        #region Site

        public ComplexVector CalculateAnomalyFieldE(ObservationSite site, AnomalyCurrent jQ)
        {
            var gt = CalculateGreenTensorAtoOSiteElectric(site);
            var result = ConvolutorE(gt, jQ);
            return result;
        }

        public ComplexVector CalculateAnomalyFieldH(ObservationSite site, AnomalyCurrent jQ)
        {
            var gt = CalculateGreenTensorAtoOSiteMagnetic(site);
            var result = CalculateH(gt, jQ);
            return result;
        }

        private GreenTensor CalculateGreenTensorAtoOSiteElectric(ObservationSite site)
        {
            var plan = _plansCreater.CreateForAnomalyToObservationSites(site);
            var transceiver = TranceiverUtils.CreateAnomalyToObservation(Model.Anomaly, site);
            var scalarCalculator = GreenScalarCalculator.NewAtoOElectricCalculator(Logger, Model);
            var scalars = scalarCalculator.Calculate(plan, transceiver);

            var calc = new AtoOSiteGreenTensorCalculator(Logger, Model, MemoryProvider);
            var segments = ScalarSegments.AllocateAndConvert(MemoryProvider, plan, scalars);

            var gt = calc.CalculateAtoOElectric(segments, site, MemoryLayoutOrder.AlongLateral);
            segments.Dispose();

            return gt;
        }

        private GreenTensor CalculateGreenTensorAtoOSiteMagnetic(ObservationSite site)
        {
            var plan = _plansCreater.CreateForAnomalyToObservationSites(site);
            var transceiver = TranceiverUtils.CreateAnomalyToObservation(Model.Anomaly, site);
            var scalarCalculator = GreenScalarCalculator.NewAtoOMagneticCalculator(Logger, Model);
            plan.CalculateI5 = false;
            var scalars = scalarCalculator.Calculate(plan, transceiver);

            var calc = new AtoOSiteGreenTensorCalculator(Logger, Model, MemoryProvider);
            var segments = ScalarSegments.AllocateAndConvert(MemoryProvider, plan, scalars);
            segments.Dispose();

            var gt = calc.CalculateAtoOMagnetic(segments, site, MemoryLayoutOrder.AlongLateral);

            return gt;
        }


        private ComplexVector ConvolutorE(GreenTensor gt, AnomalyCurrent jQ)
        {
            throw new NotImplementedException();
        }


        private ComplexVector CalculateH(GreenTensor gt, AnomalyCurrent jQ)
        {
            throw new NotImplementedException();
        }

        #endregion

        public void Dispose()
        {
            CleanGreenTensors();
        }

        public void CleanGreenTensors()
        {
            foreach (var greenTensor in _eGreenTensors.Values)
                greenTensor.Dispose();

            foreach (var greenTensor in _hGreenTensors.Values)
                greenTensor.Dispose();

            _eGreenTensors.Clear();
            _hGreenTensors.Clear();
        }
    }
}
