using System;
using System.Collections.Generic;
using Extreme.Cartesian.Logger;
using Extreme.Core;

namespace Extreme.Cartesian.Forward
{
    public abstract class ForwardSolverGenerics<T>
    {
        private readonly List<T> _allocated = new List<T>();

        protected ForwardSolverGenerics(ILogger logger, INativeMemoryProvider memoryProvider)
        {
            MemoryProvider = memoryProvider;
            Logger = logger;
        }

        protected bool SkipObservationFieldsCalculation { get; set; } = false;

        public INativeMemoryProvider MemoryProvider { get; }
        public ILogger Logger { get; }
        public IProfiler Profiler { get; private set; }

        public void SetProfiler(IProfiler profiler)
        {
            Profiler = profiler;
        }

        protected void SolveForOneSource()
        {
            using (Profiler?.StartAuto(ProfilerEvent.ForwardSolvingOneSource))
            {
                var field = CalculateFieldFromSource();


                var jScattered = CalculateJScattered(field);

                var chi0 = CalculateChi0From(jScattered);
                var chi = SolveIntegralEquationFor(chi0);

                var eScattered = CalculateEScatteredFrom(chi, jScattered);
                var jQ = CalculateJqFrom(eScattered, jScattered);

                if (!SkipObservationFieldsCalculation)
                    CalculateObservationFields(jQ);

                ReleaseAnomalyCurrent();
            }
        }

        private T CalculateFieldFromSource()
        {
            using (Profiler?.StartAuto(ProfilerEvent.SourceFieldCalculation))
            {
                Logger.WriteStatus("Calculate source field");

                var field = GetNewAnomalyCurrentAndCacheIt();
                CalculateNormalFieldFromSource(field);

                return field;
            }
        }

        protected abstract void CalculateNormalFieldFromSource(T field);


        private T CalculateJScattered(T field)
        {
            using (Profiler?.StartAuto(ProfilerEvent.CalcJScattered))
            {
                Logger.WriteStatus("Calculate J scattered");

                var jScattered = GetNewAnomalyCurrentAndCacheIt();

					
                CalculateJScattered(field, jScattered);
				return jScattered;
            }
        }

        protected abstract void CalculateJScattered(T field, T jScattered);

        private T CalculateChi0From(T jScattered)
        {
            using (Profiler?.StartAuto(ProfilerEvent.CalcChi0))
            {
                Logger.WriteStatus("Calculate chi 0");

                var chi0 = GetNewAnomalyCurrentAndCacheIt();
                CalculateChi0From(jScattered, chi0);

                return chi0;
            }
        }

        protected abstract void CalculateChi0From(T jScattered, T chi0);

        private T SolveIntegralEquationFor(T chi0)
        {
            using (Profiler?.StartAuto(ProfilerEvent.SolveCie))
            {
                Logger.WriteStatus("Solve IE equation");

                var chi = GetNewAnomalyCurrentAndCacheIt();
                SolveEquationFor(chi0, chi);

                return chi;
            }
        }

        protected abstract void SolveEquationFor(T chi0, T chi);

        private T CalculateEScatteredFrom(T chi, T jScattered)
        {
            using (Profiler?.StartAuto(ProfilerEvent.CalcEScattered))
            {
                Logger.WriteStatus("Calculate E scattered");

                var eScattrerd = GetNewAnomalyCurrentAndCacheIt();
                CalculateEScatteredFrom(chi, jScattered, eScattrerd);

                return eScattrerd;
            }
        }

        protected abstract void CalculateEScatteredFrom(T chi, T jScattered, T eScattered);

        private T CalculateJqFrom(T eScattered, T jScattered)
        {
            using (Profiler?.StartAuto(ProfilerEvent.CalcJq))
            {
                Logger.WriteStatus("Calculate J Q");

                var jQ = GetNewAnomalyCurrentAndCacheIt();
                CalculateJqFrom(eScattered, jScattered, jQ);

                return jQ;
            }
        }

        protected abstract void CalculateJqFrom(T eScattered, T jScattered, T jQ);


        protected void CalculateOnObservationsForGivenChi(T chi)
        {
            var field = CalculateFieldFromSource();
            var jScattered = CalculateJScattered(field);

            var eScattered = CalculateEScatteredFrom(chi, jScattered);
            var jQ = CalculateJqFrom(eScattered, jScattered);

            CalculateObservationFields(jQ);

            ReleaseAnomalyCurrent();
        }

        protected abstract void CalculateObservations(T jQ);

        private void CalculateObservationFields(T jQ)
        {
            using (Profiler?.StartAuto(ProfilerEvent.ObservationsFullCalculation))
            {
                CalculateObservations(jQ);
            }
        }

        private T GetNewAnomalyCurrentAndCacheIt()
        {
            var current = GetNewAnomalyCurrent();
            _allocated.Add(current);
            return current;
        }

        private void ReleaseAnomalyCurrent()
        {
            _allocated.ForEach(ReleaseAnomalyCurrent);
            _allocated.Clear();
        }

        protected abstract T GetNewAnomalyCurrent();
        protected abstract void ReleaseAnomalyCurrent(T current);
    }
}
