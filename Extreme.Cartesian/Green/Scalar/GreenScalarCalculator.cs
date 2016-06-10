using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Extreme.Cartesian.Core;
using Extreme.Cartesian.Green.Scalar.Impl;
using Extreme.Cartesian.Model;
using Extreme.Core;

namespace Extreme.Cartesian.Green.Scalar
{
    public class GreenScalarCalculator
    {
        private readonly ILogger _logger;
        private readonly OmegaModel _model;
        private readonly FieldToField _ftof;
        private readonly IntegrationType _integrationType;
        private readonly AlphaBeta _alphaBeta;

        private GreenScalarCalculator(
            ILogger logger,
            OmegaModel model,
            FieldToField ftof,
            IntegrationType integrationType)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            if (model == null) throw new ArgumentNullException(nameof(model));


            _logger = logger;
            _model = model;
            _ftof = ftof;
            _integrationType = integrationType;

            _alphaBeta = AlphaBeta.CreateFrom(model.Section1D);
        }

        public static GreenScalarCalculator NewAtoACalculator(ILogger logger, OmegaModel model)
            => new GreenScalarCalculator(logger, model, FieldToField.J2E, IntegrationType.VolumeToVolume);

        public static GreenScalarCalculator NewAtoOElectricCalculator(ILogger logger, OmegaModel model)
            => new GreenScalarCalculator(logger, model, FieldToField.J2E, IntegrationType.VolumeToPoint);

        public static GreenScalarCalculator NewAtoOMagneticCalculator(ILogger logger, OmegaModel model)
            => new GreenScalarCalculator(logger, model, FieldToField.J2H, IntegrationType.VolumeToPoint);

        public static GreenScalarCalculator NewStoAElectricCalculator(ILogger logger, OmegaModel model)
            => new GreenScalarCalculator(logger, model, FieldToField.J2E, IntegrationType.VolumeToVolume);

        public static GreenScalarCalculator NewStoAMagneticCalculator(ILogger logger, OmegaModel model)
            => new GreenScalarCalculator(logger, model, FieldToField.J2H, IntegrationType.VolumeToVolume);

        public GreenScalars Calculate(ScalarPlan plan, IEnumerable<Transceiver> transceivers, IProfiler profiler)
        {
            if (plan == null) throw new ArgumentNullException(nameof(plan));
            var trans = transceivers.ToArray();

            var notCombined = new InnerResult[trans.Length][]; //new Dictionary<Transceiver, List<InnerResult>>();

            using (profiler?.StartAuto(ProfilerEvent.GreenScalarAtoACalcCalc))
            {
                IterateParallel(trans, i =>
                {
                    notCombined[i] = CalculateByPlan(plan, trans[i].Transmitter, trans[i].Receiver);
                });
            }

            using (profiler?.StartAuto(ProfilerEvent.GreenScalarAtoAUnion))
            {
                var rho = plan.GetSortedRho();
                var combined = notCombined.Length == 0
                    ? new SingleGreenScalar[0]
                    : InnerResultsCombiner.CreateFromSample(plan, notCombined[0])
                        .CombineSeparatedScalars(trans, notCombined);

                return new GreenScalars(rho, combined);
            }
        }

        public GreenScalars Calculate(ScalarPlan plan, IEnumerable<Transceiver> transceivers)
        {
            if (plan == null) throw new ArgumentNullException(nameof(plan));
            var trans = transceivers.ToArray();

            var notCombined = new InnerResult[trans.Length][]; //new Dictionary<Transceiver, List<InnerResult>>();

            IterateParallel(trans, i =>
            {
                notCombined[i] = CalculateByPlan(plan, trans[i].Transmitter, trans[i].Receiver);
            });

            var rho = plan.GetSortedRho();

            if (notCombined.Length == 0)
                return new GreenScalars(rho, new SingleGreenScalar[0]);

            var combined = InnerResultsCombiner.CreateFromSample(plan, notCombined[0])
                .CombineSeparatedScalars(trans, notCombined);

            return new GreenScalars(rho, combined);
        }


        private void IterateParallel(Transceiver[] transceivers, Action<int> action)
        {
            var options = MultiThreadUtils.CreateParallelOptions();
            System.Threading.Tasks.Parallel.For(0, transceivers.Length, options, action);
        }

        private InnerResult[] CalculateByPlan(ScalarPlan plan, Transmitter transmitter, Receiver receiver)
        {
            var items = plan.Items;
            var separatedResults = new InnerResult[items.Length];

            for (int i = 0; i < separatedResults.Length; i++)
            {
                var calculator = GetPlanCalculator(items[i]);

                if (_ftof == FieldToField.J2E)
                    separatedResults[i] = calculator.CalculateForE(transmitter, receiver);

                if (_ftof == FieldToField.J2H)
                    separatedResults[i] = calculator.CalculateForH(transmitter, receiver);
            }

            if (plan.CalculateZeroRho)
                CalculateZeroRho(separatedResults, plan);

            return separatedResults;
        }

        private IPlanCalculator GetPlanCalculator(ScalarPlanItem item)
        {
            switch (_integrationType)
            {
                case IntegrationType.VolumeToVolume:
                    return new VolumeToVolumePlanCalculatorFast(_model, _alphaBeta, item);
                case IntegrationType.VolumeToPoint:
                    return new VolumeToPointPlanCalculator(_model, _alphaBeta, item);
                default:
                    throw new NotImplementedException(_integrationType + " not supported yet");
            }

        }

        private void CalculateZeroRho(InnerResult[] separatedResults, ScalarPlan plan)
        {
            var target = separatedResults[0];
            var rhoStep = plan.Items.First().HankelCoefficients.GetLog10RhoStep();

            if (_ftof == FieldToField.J2E)
                ScalarMathUtils.CalculateZeroRhoForE(target, rhoStep);

            if (_ftof == FieldToField.J2H)
                ScalarMathUtils.CalculateZeroRhoForH(target, rhoStep);
        }
    }
}

