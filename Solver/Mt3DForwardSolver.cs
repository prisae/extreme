using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Extreme.Cartesian;
using Extreme.Cartesian.Core;
using Extreme.Cartesian.Forward;
using Extreme.Cartesian.Green.Tensor;
using Extreme.Cartesian.Logger;
using Extreme.Cartesian.Model;
using Extreme.Cartesian.Project;
using Extreme.Core;

namespace Extreme.Cartesian.Magnetotellurics
{
    public sealed class Mt3DForwardSolver : ForwardSolver
    {
        public Polarization CurrentSource { get; private set; }
        private AnomalyCurrent _normalFieldAtAnomaly;

        private readonly List<MtFieldsAtLevelCalculatedEventArgs> _eFields;
        private readonly List<MtFieldsAtLevelCalculatedEventArgs> _hFields;

		public Mt3DForwardSolver(ILogger logger, INativeMemoryProvider memoryProvider, ForwardSettings settings,bool store=false)
            : base(logger, memoryProvider, settings,store)
        {
            _eFields = new List<MtFieldsAtLevelCalculatedEventArgs>();
            _hFields = new List<MtFieldsAtLevelCalculatedEventArgs>();
        }

        public event EventHandler<PolarizationCompleteEventArs> PolarizationComplete;
        private void OnPolarizationComplete(Polarization polarization)
            => PolarizationComplete?.Invoke(this, new PolarizationCompleteEventArs(polarization));

        public event EventHandler<EScatteredCalculatedEventArgs> EScatteredCalculated;
        public event EventHandler<MtFieldsAtLevelCalculatedEventArgs> EFieldsAtLevelCalculated;
        public event EventHandler<MtFieldsAtLevelCalculatedEventArgs> HFieldsAtLevelCalculated;
        public event EventHandler<MtFieldsAtSiteCalculatedEventArgs> EFieldsAtSiteCalculated;
        public event EventHandler<MtFieldsAtSiteCalculatedEventArgs> HFieldsAtSiteCalculated;

        #region Events Invoking
        protected override void OnEFieldsAtLevelCalculated(ObservationLevel level,
    AnomalyCurrent normalField, AnomalyCurrent anomalyField)
        {
            var e = new MtFieldsAtLevelCalculatedEventArgs(CurrentSource, level, normalField, anomalyField);
            _eFields.Add(e);
            EFieldsAtLevelCalculated?.Invoke(this, e);
        }

        protected override void OnHFieldsAtLevelCalculated(ObservationLevel level,
            AnomalyCurrent normalField, AnomalyCurrent anomalyField)
        {
            var e = new MtFieldsAtLevelCalculatedEventArgs(CurrentSource, level, normalField, anomalyField);
            _hFields.Add(e);
            HFieldsAtLevelCalculated?.Invoke(this, e);
        }

        protected override void OnEFieldsAtSiteCalculated(ObservationSite site,
            ComplexVector normalField, ComplexVector anomalyField)
        {
            var e = new MtFieldsAtSiteCalculatedEventArgs(CurrentSource, site, normalField, anomalyField);
            EFieldsAtSiteCalculated?.Invoke(this, e);
        }

        protected override void OnHFieldsAtSiteCalculated(ObservationSite site,
            ComplexVector normalField, ComplexVector anomalyField)
        {
            var e = new MtFieldsAtSiteCalculatedEventArgs(CurrentSource, site, normalField, anomalyField);
            HFieldsAtSiteCalculated?.Invoke(this, e);
        }
        protected override unsafe void OnEScatteredCalculated(AnomalyCurrent eScattered)
        {
            EScatteredCalculated?.Invoke(this, new EScatteredCalculatedEventArgs(_normalFieldAtAnomaly, eScattered));
        }

        #endregion

        public ResultsContainer Solve(OmegaModel model, GreenTensor aToA = null)
        {
            SolvePrivate(model, aToA);

            return GatherSolution();
        }

		public ResultsContainer SolveWithoutGather(OmegaModel model, TensorCache tensors=null)
        {
			
			if (tensors==null)
				SolvePrivate(model, null,tensors);
			else
				SolvePrivate(model, tensors.gtAtoA,tensors);
            return GatherSolutionLocally();
        }

		private void SolvePrivate(OmegaModel model, GreenTensor aToA,TensorCache tensors=null)
        {
            Logger.WriteStatus("Starting Cartesian MT Forward solver...");
		
            using (Profiler?.StartAuto(ProfilerEvent.ForwardSolving))
            {
				SetModel(model,tensors);

				if (aToA == null)
					CalculateGreenTensor ();
				else {
					SetNewGreenTensor (aToA);
				}
				
                SolverPolarizationX();
                SolverPolarizationY();
            }
        }


        private void SolverPolarizationX()
        {
            Logger.WriteStatus("Starting Polarization X");
            CurrentSource = Polarization.X;
            SolveForOneSource();
            OnPolarizationComplete(CurrentSource);
        }

        private void SolverPolarizationY()
        {
            Logger.WriteStatus("Starting Polarization Y");
            CurrentSource = Polarization.Y;
            SolveForOneSource();
            OnPolarizationComplete(CurrentSource);
        }


        public void Solve(AnomalyCurrent chiX, AnomalyCurrent chiY)
        {
            Logger.WriteStatus("Starting Cartesian MT Forward solver...");

            Logger.WriteStatus("\n\nStarting Polarization X\n\n");
            CurrentSource = Polarization.X;
            CalculateOnObservationsForGivenChi(chiX);

            Logger.WriteStatus("\n\nStarting Polarization Y\n\n");
            CurrentSource = Polarization.Y;
            CalculateOnObservationsForGivenChi(chiY);
        }

        private ResultsContainer GatherSolution()
        {
            Logger.WriteStatus("Gather results...");

            var rc = new ResultsContainer(Model.LateralDimensions);

            if (!IsParallel || Mpi.Rank < Mpi.Size / 2)
            {
                foreach (var observationLevel in _observationLevels)
                {
                    var all = GatherAllFieldsAtLevel(observationLevel, _eFields, _hFields);

                    if (!IsParallel || Mpi.IsMaster)
                        rc.Add(all);
                }
            }

            ClearLocalCalculatedFields();

            return rc;
        }

        private ResultsContainer GatherSolutionLocally()
        {
			var nx = Model.Anomaly.LocalSize.Nx;
			int nx_offset = 0;
			if (IsParallel) {
				nx = Model.Anomaly.LocalSize.Nx;
				nx_offset = Mpi.CalcLocalHalfNxStart (Model.Nx);
			}
			var rc = new ResultsContainer(Model.LateralDimensions,nx,nx_offset);
			if (nx > 0) {          
				foreach (var observationLevel in _observationLevels) {
					var all = GatherAllFieldsAtLevelLocally (observationLevel, _eFields, _hFields);
					rc.Add (all);
                
				}
			}
            _eFields.Clear();
            _hFields.Clear();
            
            return rc;
        }


        private void ClearLocalCalculatedFields()
        {
            if (IsParallel)
            {
                _eFields.ForEach(f =>
                {
                    f.NormalField.Dispose();
                    f.AnomalyField.Dispose();
                });

                _hFields.ForEach(f =>
                {
                    f.NormalField.Dispose();
                    f.AnomalyField.Dispose();
                });
            }

            _eFields.Clear();
            _hFields.Clear();
        }

        private AllFieldsAtLevel GatherAllFieldsAtLevel(ObservationLevel level, List<MtFieldsAtLevelCalculatedEventArgs> eFields, List<MtFieldsAtLevelCalculatedEventArgs> hFields)
        {
            var e1 = eFields.First(e => e.Level == level && e.Polarization == Polarization.X);
            var e2 = eFields.First(e => e.Level == level && e.Polarization == Polarization.Y);
            var h1 = hFields.First(h => h.Level == level && h.Polarization == Polarization.X);
            var h2 = hFields.First(h => h.Level == level && h.Polarization == Polarization.Y);

            return new AllFieldsAtLevel(level)
            {
                AnomalyE1 = DistibutedUtils.GatherFromAllProcesses(this, e1.AnomalyField),
                NormalE1 = DistibutedUtils.GatherFromAllProcesses(this, e1.NormalField),
                AnomalyE2 = DistibutedUtils.GatherFromAllProcesses(this, e2.AnomalyField),
                NormalE2 = DistibutedUtils.GatherFromAllProcesses(this, e2.NormalField),

                AnomalyH1 = DistibutedUtils.GatherFromAllProcesses(this, h1.AnomalyField),
                NormalH1 = DistibutedUtils.GatherFromAllProcesses(this, h1.NormalField),
                AnomalyH2 = DistibutedUtils.GatherFromAllProcesses(this, h2.AnomalyField),
                NormalH2 = DistibutedUtils.GatherFromAllProcesses(this, h2.NormalField),
            };
        }

        private AllFieldsAtLevel GatherAllFieldsAtLevelLocally(ObservationLevel level, List<MtFieldsAtLevelCalculatedEventArgs> eFields, List<MtFieldsAtLevelCalculatedEventArgs> hFields)
        {
            var e1 = eFields.First(e => e.Level == level && e.Polarization == Polarization.X);
            var e2 = eFields.First(e => e.Level == level && e.Polarization == Polarization.Y);
            var h1 = hFields.First(h => h.Level == level && h.Polarization == Polarization.X);
            var h2 = hFields.First(h => h.Level == level && h.Polarization == Polarization.Y);

            return new AllFieldsAtLevel(level)
            {
                AnomalyE1 = e1.AnomalyField,
                NormalE1 = e1.NormalField,
                AnomalyE2 = e2.AnomalyField,
                NormalE2 = e2.NormalField,

                AnomalyH1 = h1.AnomalyField,
                NormalH1 = h1.NormalField,
                AnomalyH2 = h2.AnomalyField,
                NormalH2 = h2.NormalField,
            };
        }


        #region Source

        protected override void CalculateNormalFieldFromSource(AnomalyCurrent field)
        {
            Clear(field);

            for (int k = 0; k < field.Nz; k++)
            {
                var mtField = CalculateEFieldForPlaneWaveOnAnomalies(Model, Model.Anomaly.Layers[k]);

                var laS = CurrentSource == Polarization.X ?
                            GetLayerAccessorX(field, k) :
                            GetLayerAccessorY(field, k);

                for (int i = 0; i < laS.Nx; i++)
                    for (int j = 0; j < laS.Ny; j++)
                        laS[i, j] = mtField;
            }

            _normalFieldAtAnomaly = field;
        }

        private static Complex CalculateEFieldForPlaneWaveOnAnomalies(OmegaModel model, IAnomalyLayer layer)
        {
            var reciever = Receiver.NewVolumetric(layer.Depth, layer.Thickness);
            return PlaneWaveCalculator.CalculateFieldE(model, reciever.GetWorkingDepth());
        }

        #endregion

        #region Observations

        protected override ComplexVector CalculateNormalFieldE(ObservationSite site)
        {
            var impedance = PlaneWaveCalculator.CalculateFieldE(Model, site.Z);
            if (CurrentSource == Polarization.X) return new ComplexVector(impedance, 0, 0);
            if (CurrentSource == Polarization.Y) return new ComplexVector(0, impedance, 0);
            throw new InvalidOperationException();
        }

        protected override ComplexVector CalculateNormalFieldH(ObservationSite site)
        {
            var admitance = PlaneWaveCalculator.CalculateFieldH(Model, site.Z);
            if (CurrentSource == Polarization.X) return new ComplexVector(0, admitance, 0);
            if (CurrentSource == Polarization.Y) return new ComplexVector(-admitance, 0, 0);
            throw new InvalidOperationException();
        }

        protected override AnomalyCurrent CalculateNormalFieldE(ObservationLevel level)
        {
            var normalField = AnomalyCurrent.AllocateNewOneLayer(MemoryProvider, Model);
            Clear(normalField);
            var mtImpedanceOnObs = PlaneWaveCalculator.CalculateFieldE(Model, level.Z);

            if (CurrentSource == Polarization.X)
            {
                var la = GetLayerAccessorX(normalField, 0);
                SetValue(la, mtImpedanceOnObs);
            }

            if (CurrentSource == Polarization.Y)
            {
                var la = GetLayerAccessorY(normalField, 0);
                SetValue(la, mtImpedanceOnObs);
            }

            return normalField;
        }

        protected override AnomalyCurrent CalculateNormalFieldH(ObservationLevel level)
        {
            var normalField = AnomalyCurrent.AllocateNewOneLayer(MemoryProvider, Model);
            Clear(normalField);
            var mtAdmitanceOnObs = PlaneWaveCalculator.CalculateFieldH(Model, level.Z);

            if (CurrentSource == Polarization.X)
            {
                var la = GetLayerAccessorY(normalField, 0);
                SetValue(la, mtAdmitanceOnObs);
            }

            if (CurrentSource == Polarization.Y)
            {
                var la = GetLayerAccessorX(normalField, 0);
                SetValue(la, -mtAdmitanceOnObs);
            }

            return normalField;
        }

        private void SetValue(ILayerAccessor la, Complex value)
        {
            int localNx = Model.Anomaly.LocalSize.Nx;
            int localNy = Model.Anomaly.LocalSize.Ny;

            for (int i = 0; i < localNx; i++)
                for (int j = 0; j < localNy; j++)
                    la[i, j] = value;
        }

        #endregion
    }
}
