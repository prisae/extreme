using System;
using System.Collections.Generic;
using System.Numerics;
using Extreme.Cartesian.Convolution;
using Extreme.Cartesian.Core;
using Extreme.Cartesian.Fft;
using Extreme.Cartesian.Green.Tensor;
using Extreme.Cartesian.Logger;
using Extreme.Cartesian.Model;
using Extreme.Cartesian.Project;
using Extreme.Core;
using Extreme.Core.Model;
using Extreme.Parallel;
using Microsoft.SqlServer.Server;
using Extreme.Cartesian.Green;
using Extreme.Cartesian.Giem2g;
using System.Runtime.Remoting.Channels;
using System.Globalization;

namespace Extreme.Cartesian.Forward
{
    public abstract unsafe class ForwardSolver : ForwardSolverGenerics<AnomalyCurrent>, IDisposable
    {
        private GreenTensor _greenTensorAtoA=null;
        private ConvolutionOperator _convolutionOperator;
        private AnomalyCurrentFgmresSolver _fgmresSolver;


		public  ForwardSolverEngine Engine { get; private set; }= ForwardSolverEngine.X3dScattered;



        private AtoOCalculator _aToOCalculator;

        protected IEnumerable<ObservationLevel> _observationLevels = new ObservationLevel[0];
        protected IEnumerable<ObservationSite> _observationSites = new ObservationSite[0];

        public ForwardSettings Settings { get; }
        public OmegaModel Model { get; private set; }
		public FftBuffer Pool => FftBuffersPool.GetBuffer(Model);

        public Mpi Mpi { get; private set; }
		public Mpi MpiRealPart =>Pool.RealModelPart;
		        
        public bool IsParallel => Mpi != null && Mpi.IsParallel;

		private bool StoreAtoA=false;


        private int LocalNx => Model.Anomaly.LocalSize.Nx;
        private int LocalNy => Model.Anomaly.LocalSize.Ny;

		protected ForwardSolver(ILogger logger, INativeMemoryProvider memoryProvider, ForwardSettings settings,bool store=false)
            : base(logger, memoryProvider)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            Settings = settings;
			StoreAtoA=store;
			if (Settings.NumberOfHankels <=0) {
				Engine=ForwardSolverEngine.Giem2g;

				SetGiem2gLogger();

				StoreAtoA=true;
			}
        }

        public ForwardSolver WithMpi(Mpi mpi)
        {
            Mpi = mpi;
            return this;
        }

		public ForwardSolver With(ForwardSolverEngine engine)
		{
			Engine = engine;
			_greenTensorAtoA?.Dispose();
			if (Engine == ForwardSolverEngine.Giem2g) {
				SetGiem2gLogger();
				StoreAtoA=true;
			} else {
				StoreAtoA=false;
			}
			
			return this;
		}


        public ForwardSolver With(params ObservationLevel[] levels)
        {
            if (levels == null) throw new ArgumentNullException(nameof(levels));
            _observationLevels = levels;
            return this;
        }

        public ForwardSolver With(params ObservationSite[] sites)
        {
            if (sites == null) throw new ArgumentNullException(nameof(sites));
            _observationSites = sites;
            return this;
        }

        protected override sealed AnomalyCurrent GetNewAnomalyCurrent()
            => AnomalyCurrent.AllocateNewLocalSize(MemoryProvider, Model);
        sealed protected override void ReleaseAnomalyCurrent(AnomalyCurrent current)
            => current.Dispose();

        public event EventHandler<AtoAGreenTensorCalculatedEventArgs> AtoAGreenTensorCalculated;

        private void OnAtoAGreenTensorCalculated(AtoAGreenTensorCalculatedEventArgs e)
        {
            AtoAGreenTensorCalculated?.Invoke(this, e);
        }

		private void Giem2gLoggerRequest(object sender, GIEM2GLoggerEventArgs e){
				Logger.WriteStatus (e.Message);
		}

		public void SetGiem2gLogger(){
			if (Engine == ForwardSolverEngine.Giem2g) {
				Giem2gGreenTensor.giem2g_set_logger(Giem2gGreenTensor.GIEM2G_LOGGER);
				Giem2gGreenTensor.GIEM2G_Message += Giem2gLoggerRequest;
				CieSolverFinished += Giem2gGreenTensor.PrintStats;
			}
		}


		public void RemoveGiem2gLogger(){
			if (Engine == ForwardSolverEngine.Giem2g) {
				Giem2gGreenTensor.GIEM2G_Message -= Giem2gLoggerRequest;
				CieSolverFinished -= Giem2gGreenTensor.PrintStats;
			}
		}


		protected void SetModel(OmegaModel model, TensorCache tensors=null)
        {
            if (IsParallel)
                Mpi.CheckNumberOfProcesses(model.LateralDimensions);

            Model = model;

            if (_aToOCalculator == null)
                _aToOCalculator = new AtoOCalculator(this);

            if (_fgmresSolver == null)
                _fgmresSolver = new AnomalyCurrentFgmresSolver(this);

			if (_convolutionOperator == null) 
				_convolutionOperator = new ConvolutionOperator (this);

			if (tensors == null) {
				_aToOCalculator.CleanGreenTensors ();
				if (!StoreAtoA)
					_greenTensorAtoA?.Dispose();
			} else {
				_aToOCalculator.SetTensors (tensors.eGreenTensors, tensors.hGreenTensors);
				SetNewGreenTensor(tensors.gtAtoA);
			}
        }

        protected void CalculateGreenTensor()
        {
            Logger.WriteStatus("Starting Green Tensor AtoA");
			if (!StoreAtoA)
					_greenTensorAtoA?.Dispose();
			var	gt = new AtoAGreenTensorCalculatorComponent (this)
				.CalculateGreenTensor (_greenTensorAtoA);
			
            OnAtoAGreenTensorCalculated(new AtoAGreenTensorCalculatedEventArgs(gt));

            SetNewGreenTensor(gt);
        }

        protected void SetNewGreenTensor(GreenTensor gt)
        {
            if (gt == null) throw new ArgumentNullException(nameof(gt));
            _greenTensorAtoA = gt;
        }

        #region Before CIE

        sealed protected override void CalculateJScattered(AnomalyCurrent field, AnomalyCurrent jScattered)
        {
            Clear(jScattered);
			var anom = Model.Anomaly;

			for (int k = 0; k < anom.Layers.Count; k++) {
				var corrLayer = ModelUtils.FindCorrespondingBackgroundLayer (Model.Section1D, anom.Layers [k]);
				var zetaBackground = corrLayer.Zeta;
				int layerIndex = k;

				if (Engine == ForwardSolverEngine.X3dScattered) {
					CalculateScatteredCurrentFromBackgroundField (anom.Zeta, k, field, zetaBackground, jScattered, ac => GetLayerAccessorX (ac, layerIndex));
					CalculateScatteredCurrentFromBackgroundField (anom.Zeta, k, field, zetaBackground, jScattered, ac => GetLayerAccessorY (ac, layerIndex));
					CalculateScatteredCurrentFromBackgroundField (anom.Zeta, k, field, zetaBackground, jScattered, ac => GetLayerAccessorZ (ac, layerIndex));
				} else {
					CalculateChi0FromBackgroundField (field, zetaBackground, jScattered, ac => GetLayerAccessorX (ac, layerIndex));
					CalculateChi0FromBackgroundField (field, zetaBackground, jScattered, ac => GetLayerAccessorY (ac, layerIndex));
					CalculateChi0FromBackgroundField (field, zetaBackground, jScattered, ac => GetLayerAccessorZ (ac, layerIndex));
				}
			}
			
        }


        /// <summary>
        /// Calculates scattered current from background field
        /// jˢ = (σ - σᵇ) * Eᵇ
        /// </summary>
        /// <param name="k"></param>
        /// <param name="field">background field</param>
        /// <param name="zeta">Background or Host zeta (1-D, σᵇ)</param>
        /// <param name="jScattered">result, scattered current</param>
        /// <param name="getLa"></param>
        /// <param name="zetas"></param>
		/// 
		/// 



        private void CalculateScatteredCurrentFromBackgroundField(Complex[,,] zetas, int k,
            AnomalyCurrent field, Complex zeta, AnomalyCurrent jScattered, Func<AnomalyCurrent, ILayerAccessor> getLa)
        {
            var nx = LocalNx;
            var ny = LocalNy;

            var jS = getLa(jScattered);
            var f = getLa(field);

            for (int i = 0; i < nx; i++)
                for (int j = 0; j < ny; j++)
                    jS[i, j] = (zetas[i,j,k] - zeta) * f[i, j];
        }

		private void CalculateChi0FromBackgroundField(AnomalyCurrent field, Complex zeta, AnomalyCurrent jScattered, Func<AnomalyCurrent, ILayerAccessor> getLa)
		{
			var nx = LocalNx;
			var ny = LocalNy;

			var jS = getLa(jScattered);
			var f = getLa(field);

			for (int i = 0; i < nx; i++)
				for (int j = 0; j < ny; j++)
					jS[i, j] = Complex.Sqrt(zeta.Real) * f[i, j];
		}
        

        sealed protected override void CalculateChi0From(AnomalyCurrent jScattered, AnomalyCurrent chi0)
        {
			if (Engine == ForwardSolverEngine.X3dScattered) {
				_convolutionOperator.PrepareOperator (_greenTensorAtoA, OperatorType.Chi0);
				_convolutionOperator.Apply (jScattered, chi0);
			}else{
				if (Engine == ForwardSolverEngine.Giem2g) {
					var nx = Model.Anomaly.LocalSize.Nx;
					var ny = Model.Anomaly.LocalSize.Ny;
					var nz = Model.Nz;
					long ind1 = 0;
					long ind2 = 0;
					for (int k = 0; k < 3*nz; k++)
						for (int i = 0; i < nx; i++)
							for (int j = 0; j < ny; j++)
							{
								ind2 = k + 3 * nz * (j + i * ny);
								chi0.Ptr[ind1++] =jScattered.Ptr[ind2];
							}
					//Copy (jScattered, chi0);	
				}
			}
        }



		#endregion

		#region CIE Solving

        public event EventHandler<CieSolverStartedEventArgs> CieSolverStarted;
        public event EventHandler<CieSolverFinishedEventArgs> CieSolverFinished;

        private void OnCieSolverStarted(AnomalyCurrent rhs, AnomalyCurrent initialGuess)
        {
            CieSolverStarted?.Invoke(this, new CieSolverStartedEventArgs(rhs, initialGuess));
        }

        private void OnCieSolverFinished(AnomalyCurrent chi)
        {
			if (Engine != ForwardSolverEngine.Giem2g) {
				CieSolverFinished?.Invoke (this, new CieSolverFinishedEventArgs (chi));
			}else{
				CieSolverFinished?.Invoke (this, new CieSolverFinishedEventArgs (chi, _greenTensorAtoA));
			}
        }

        sealed protected override void SolveEquationFor(AnomalyCurrent chi0, AnomalyCurrent chi)
        {
            _convolutionOperator.PrepareOperator(_greenTensorAtoA, OperatorType.A);
            Copy(chi0, chi);
            OnCieSolverStarted(chi0, chi);
            _fgmresSolver.Solve(_convolutionOperator, chi0, chi);
            OnCieSolverFinished(chi);
        }

        private void Copy(AnomalyCurrent chi0, AnomalyCurrent chi)
        {
            long size = 3L * chi0.Nx * chi0.Ny * chi0.Nz;
            UnsafeNativeMethods.Zcopy(size, chi0.Ptr, chi.Ptr);
        }

        #endregion

        #region After CIE, Before Observations

        sealed protected override void CalculateEScatteredFrom(AnomalyCurrent chi, AnomalyCurrent jScattered, AnomalyCurrent eScattered)
        {

			if (Engine == ForwardSolverEngine.Giem2g) {
				var nx = Model.Anomaly.LocalSize.Nx;
				var ny = Model.Anomaly.LocalSize.Ny;
				var nz = Model.Nz;
				long ind1 = 0;
				long ind2 = 0;
				for (int i = 0; i < nx; i++)
					for (int j = 0; j < ny; j++)
						for (int k = 0; k < 3*nz; k++)
						{
							ind2 = j + ny * (i + k * nx);
							eScattered.Ptr[ind1++] =chi.Ptr[ind2];
						}
				Copy (eScattered,chi);
			}




            for (int k = 0; k < Model.Anomaly.Layers.Count; k++)
            {
                var layer = Model.Anomaly.Layers[k];
                var zetas = Model.Anomaly.Zeta;
                var corrLayer = ModelUtils.FindCorrespondingBackgroundLayer(Model.Section1D, layer);
                var zeta = corrLayer.Zeta;
                int layerIndex = k;
				if (Engine == ForwardSolverEngine.X3dScattered) {
					CalculateScatteredFieldFromChi (chi, jScattered, zetas, k, zeta, eScattered, ac => GetLayerAccessorX (ac, layerIndex));
					CalculateScatteredFieldFromChi (chi, jScattered, zetas, k, zeta, eScattered, ac => GetLayerAccessorY (ac, layerIndex));
					CalculateScatteredFieldFromChi (chi, jScattered, zetas, k, zeta, eScattered, ac => GetLayerAccessorZ (ac, layerIndex));
				}else{
					CalculateTotalFieldFromChi (chi,  zetas, k, zeta, eScattered, ac => GetLayerAccessorX (ac, layerIndex));
					CalculateTotalFieldFromChi (chi,  zetas, k, zeta, eScattered, ac => GetLayerAccessorY (ac, layerIndex));
					CalculateTotalFieldFromChi (chi,  zetas, k, zeta, eScattered, ac => GetLayerAccessorZ (ac, layerIndex));
				}
            }

            OnEScatteredCalculated(eScattered);
        }

        protected virtual void OnEScatteredCalculated(AnomalyCurrent eScattered) { }

        sealed protected override void CalculateJqFrom(AnomalyCurrent eScattered, AnomalyCurrent jScattered, AnomalyCurrent jQ)
        {
            var anom = Model.Anomaly;





            for (int k = 0; k < Model.Anomaly.Layers.Count; k++)
            {
                var corrLayer = ModelUtils.FindCorrespondingBackgroundLayer(Model.Section1D, anom.Layers[k]);
                var zeta = corrLayer.Zeta;
                int layerIndex = k;
				if (Engine == ForwardSolverEngine.X3dScattered) {
                	CalculateJqFromScatteredField(anom.Zeta, k, eScattered, jScattered, zeta, jQ, ac => GetLayerAccessorX(ac, layerIndex));
                	CalculateJqFromScatteredField(anom.Zeta, k, eScattered, jScattered, zeta, jQ, ac => GetLayerAccessorY(ac, layerIndex));
                	CalculateJqFromScatteredField(anom.Zeta, k, eScattered, jScattered, zeta, jQ, ac => GetLayerAccessorZ(ac, layerIndex));
				}else{
					CalculateJqFromTotalField(anom.Zeta, k, eScattered,  zeta, jQ, ac => GetLayerAccessorX(ac, layerIndex));
					CalculateJqFromTotalField(anom.Zeta, k, eScattered,  zeta, jQ, ac => GetLayerAccessorY(ac, layerIndex));
					CalculateJqFromTotalField(anom.Zeta, k, eScattered,  zeta, jQ, ac => GetLayerAccessorZ(ac, layerIndex));
				}
            }

		
        }

        private void CalculateScatteredFieldFromChi(AnomalyCurrent chi, AnomalyCurrent jScattered, Complex[,,] zetas, int k, Complex zeta, AnomalyCurrent eScattered, Func<AnomalyCurrent, ILayerAccessor> getLa)
        {
            var nx = LocalNx;
            var ny = LocalNy;

            var conjZeta = Complex.Conjugate(zeta);
            var sqrtReZeta0 = Complex.Sqrt(zeta.Real);

            var laE = getLa(eScattered);
            var laJ = getLa(jScattered);
            var laC = getLa(chi);

            for (int i = 0; i < nx; i++)
                for (int j = 0; j < ny; j++)
                    laE[i, j] = (2 * sqrtReZeta0 * laC[i, j] - laJ[i, j]) / (zetas[i, j, k] + conjZeta);
        }

		private void CalculateTotalFieldFromChi(AnomalyCurrent chi,  Complex[,,] zetas, int k, Complex zeta, AnomalyCurrent eScattered, Func<AnomalyCurrent, ILayerAccessor> getLa)
		{
			var nx = LocalNx;
			var ny = LocalNy;

			var conjZeta = Complex.Conjugate(zeta);
			var sqrtReZeta0 = Complex.Sqrt(zeta.Real);

			var laE = getLa(eScattered);
			var laC = getLa(chi);

			for (int i = 0; i < nx; i++)
				for (int j = 0; j < ny; j++)
					laE[i, j] = (2 * sqrtReZeta0 * laC[i, j] ) / (zetas[i, j, k] + conjZeta);
		}

        private void CalculateJqFromScatteredField(Complex[,,] zetas, int k, AnomalyCurrent eScattered, AnomalyCurrent jScattered, Complex zeta, AnomalyCurrent jQ, Func<AnomalyCurrent, ILayerAccessor> getLa)
        {
            var nx = LocalNx;
            var ny = LocalNy;

            var laE = getLa(eScattered);
            var laJ = getLa(jScattered);
            var laQ = getLa(jQ);

            for (int i = 0; i < nx; i++)
                for (int j = 0; j < ny; j++)
                    laQ[i, j] = laE[i, j] * (zetas[i, j, k] - zeta) + laJ[i, j];
        }

		private void CalculateJqFromTotalField(Complex[,,] zetas, int k, AnomalyCurrent eScattered, Complex zeta, AnomalyCurrent jQ, Func<AnomalyCurrent, ILayerAccessor> getLa)
		{
			var nx = LocalNx;
			var ny = LocalNy;

			var laE = getLa(eScattered);
			var laQ = getLa(jQ);

			for (int i = 0; i < nx; i++)
				for (int j = 0; j < ny; j++)
					laQ[i, j] = laE[i, j] * (zetas[i, j, k] - zeta);
		}

        #endregion

        #region Observations

        protected virtual void OnEFieldsAtLevelCalculated(ObservationLevel level,
            AnomalyCurrent normalField, AnomalyCurrent anomalyField)
        { }

        protected virtual void OnHFieldsAtLevelCalculated(ObservationLevel level,
            AnomalyCurrent normalField, AnomalyCurrent anomalyField)
        { }

        protected virtual void OnEFieldsAtSiteCalculated(ObservationSite site,
            ComplexVector normalField, ComplexVector anomalyField)
        { }

        protected virtual void OnHFieldsAtSiteCalculated(ObservationSite site,
            ComplexVector normalField, ComplexVector anomalyField)
        { }


        protected virtual void OnObservationsCalculating()
        {

        }

        sealed protected override void CalculateObservations(AnomalyCurrent jQ)
        {
            OnObservationsCalculating();


            _aToOCalculator.GreenTensorECalculated += AToO_GreenTensorECalculated;
            _aToOCalculator.GreenTensorHCalculated += AToO_GreenTensorHCalculated;

            CalculateLevelElectric(jQ);
            CalculateLevelMagnetic(jQ);

            CalculateSiteElectric(_aToOCalculator, jQ);
            CalculateSiteMagnetic(_aToOCalculator, jQ);

            _aToOCalculator.GreenTensorECalculated -= AToO_GreenTensorECalculated;
            _aToOCalculator.GreenTensorHCalculated -= AToO_GreenTensorHCalculated;
        }

        public event EventHandler<GreenTensorCalculatedEventArgs> AtoOGreenTensorECalculated;
        public event EventHandler<GreenTensorCalculatedEventArgs> AtoOGreenTensorHCalculated;

        private void AToO_GreenTensorHCalculated(object sender, GreenTensorCalculatedEventArgs e)
        {
            e.SupressGreenTensorDisposal = true;
            AtoOGreenTensorHCalculated?.Invoke(this, e);
        }

        private void AToO_GreenTensorECalculated(object sender, GreenTensorCalculatedEventArgs e)
        {
            e.SupressGreenTensorDisposal = true;
            AtoOGreenTensorECalculated?.Invoke(this, e);
        }

        #region Site

        private void CalculateSiteElectric(AtoOCalculator aToO, AnomalyCurrent jQ)
        {
            foreach (var site in _observationSites)
            {
                var normalField = CalculateNormalFieldE(site);
                // var anomalyField = CalculateAnomalyFieldE(site, jQ);

                // OnEFieldsAtSiteCalculated(site, normalField, anomalyField);
            }
        }

        private void CalculateSiteMagnetic(AtoOCalculator aToO, AnomalyCurrent jQ)
        {
            foreach (var site in _observationSites)
            {
                var normalField = CalculateNormalFieldH(site);
                // var anomalyField = CalculateAnomalyFieldH(site, jQ);

                //  OnHFieldsAtSiteCalculated(site, normalField, anomalyField);
            }
        }

        protected abstract ComplexVector CalculateNormalFieldE(ObservationSite site);
        protected abstract ComplexVector CalculateNormalFieldH(ObservationSite site);


        #endregion

        #region Level
        private void CalculateLevelElectric(AnomalyCurrent jQ)
        {
            foreach (var level in _observationLevels)
            {
                var normalField = CalculateNormalFieldE(level);
                var anomalyField = CalculateAnomalyFieldE(level, jQ);

                OnEFieldsAtLevelCalculated(level, normalField, anomalyField);
            }
        }

        private void CalculateLevelMagnetic(AnomalyCurrent jQ)
        {
            foreach (var level in _observationLevels)
            {
                var normalField = CalculateNormalFieldH(level);
                var anomalyField = CalculateAnomalyFieldH(level, jQ);

                OnHFieldsAtLevelCalculated(level, normalField, anomalyField);
            }
        }

        private AnomalyCurrent CalculateAnomalyFieldH(ObservationLevel level, AnomalyCurrent jQ)
        {
            var field = AnomalyCurrent.AllocateNewOneLayer(MemoryProvider, Model);
            Clear(field);
            _aToOCalculator.CalculateAnomalyFieldH(level, jQ, field);
            return field;
        }

        private AnomalyCurrent CalculateAnomalyFieldE(ObservationLevel level, AnomalyCurrent jQ)
        {
            var field = AnomalyCurrent.AllocateNewOneLayer(MemoryProvider, Model);
            Clear(field);
            _aToOCalculator.CalculateAnomalyFieldE(level, jQ, field);
            return field;
        }

        protected abstract AnomalyCurrent CalculateNormalFieldE(ObservationLevel level);
        protected abstract AnomalyCurrent CalculateNormalFieldH(ObservationLevel level);


        #endregion

        #endregion

        #region Utils

        protected ILayerAccessor GetLayerAccessorX(AnomalyCurrent ac, int k)
            => VerticalLayerAccessor.NewX(ac, k);
        protected ILayerAccessor GetLayerAccessorY(AnomalyCurrent ac, int k)
            => VerticalLayerAccessor.NewY(ac, k);
        protected ILayerAccessor GetLayerAccessorZ(AnomalyCurrent ac, int k)
            => VerticalLayerAccessor.NewZ(ac, k);

        #endregion

        protected void Clear(AnomalyCurrent current)
        {
            int length = 3 * current.Nx * current.Ny * current.Nz;
            UnsafeNativeMethods.ClearBuffer(current.Ptr, length);
        }

        public void Dispose()
        {
            _convolutionOperator?.Dispose();
            _fgmresSolver?.Dispose();
        }
    }
}
