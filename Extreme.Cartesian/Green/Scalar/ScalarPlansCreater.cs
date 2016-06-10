using System;
using System.Collections.Generic;
using System.Linq;
using Extreme.Core;
using Extreme.Cartesian.Green;
using static System.Math;

namespace Extreme.Cartesian.Green.Scalar
{
    public static class ScalarPlansCreaterUtils
    {
        public static bool IsBetween(this decimal value, decimal left, decimal right)
            => value >= left && value <= right;
    }

    public class ScalarPlansCreater
    {
        private readonly LateralDimensions _lateral;
        private readonly HankelCoefficients _hankel;
        private readonly int _numberOfHankels;

        public ScalarPlansCreater(LateralDimensions lateral, HankelCoefficients hankel, int numberOfHankels)
        {
            _lateral = lateral;
            _hankel = hankel;

			_numberOfHankels = Math.Abs(numberOfHankels);
        }

        /// <summary>
        /// If rhoMin == 0 then rhoMin = lateralDimension * RhoMinForcedFactor
        /// </summary>
        private const double RhoMinForcedFactor = 0.001;

        public ScalarPlan CreateForAnomalyToObservationSites(params ObservationSite[] sites)
        {
            if (sites.Length == 0) throw new ArgumentOutOfRangeException(nameof(sites));

            var rhoMin = FindRhoMin(_lateral, sites);
            var rhoMax = FindRhoMax(_lateral, sites);

            bool addZeroRho = rhoMin <= 0;

            if (addZeroRho)
                rhoMin = (double)Min(_lateral.CellSizeX, _lateral.CellSizeY) * RhoMinForcedFactor;

            return CreateFor(rhoMin, rhoMax, addZeroRho);
        }

        public ScalarPlan CreateForAnomalyToObservationLevels(params ObservationLevel[] levels)
        {
            if (levels == null) throw new ArgumentNullException(nameof(levels));
            if (levels.Length == 0)
                throw new InvalidOperationException($"{nameof(levels)}.Length == 0");

            var rhoMin = FindRhoMin(_lateral, levels);
            var rhoMax = FindRhoMax(_lateral, levels);

            bool addZeroRho = rhoMin <= 0;

            if (addZeroRho)
                rhoMin = (double)Min(_lateral.CellSizeX, _lateral.CellSizeY) * RhoMinForcedFactor;

            return CreateFor(rhoMin, rhoMax, addZeroRho);
        }

        public ScalarPlan CreateForSourceToAnomaly(params SourceLayer[] layers)
        {
            if (layers == null) throw new ArgumentNullException(nameof(layers));
            if (layers.Length == 0)
                throw new InvalidOperationException("layers.Length == 0");

            var rhoMin = FindRhoMin(_lateral, layers);
            var rhoMax = FindRhoMax(_lateral, layers);

            bool addZeroRho = rhoMin <= 0;

            if (addZeroRho)
                rhoMin = (double)Min(_lateral.CellSizeX, _lateral.CellSizeY) * RhoMinForcedFactor;

            return CreateFor(rhoMin, rhoMax, addZeroRho);
        }

        public ScalarPlan CreateForSourceToObservation(SourceLayer src, params ObservationLevel[] levels)
        {
            if (levels == null) throw new ArgumentNullException(nameof(levels));
            if (levels.Length == 0)
                throw new InvalidOperationException("layers.Length == 0");

            var rhoMin = FindRhoMin(_lateral, src, levels);
            var rhoMax = FindRhoMax(_lateral, src, levels);

            bool addZeroRho = rhoMin <= 0;

            if (addZeroRho)
                rhoMin = (double)Min(_lateral.CellSizeX, _lateral.CellSizeY) * RhoMinForcedFactor;

            return CreateFor(rhoMin, rhoMax, addZeroRho);
        }

        public ScalarPlan CreateForAnomalyToAnomaly()
        {
            var rhoMin = FindRhoMin(_lateral);
            var rhoMax = FindRhoMax(_lateral);

            return CreateFor(rhoMin, rhoMax, addZeroRho: false);
        }

        private ScalarPlan CreateFor(double rhoMin, double rhoMax, bool addZeroRho)
        {
            if (rhoMin <= 0)
                throw new ArgumentOutOfRangeException(nameof(rhoMin));

            float rhoStep = _hankel.GetLog10RhoStep();
            var result = new ScalarPlan(addZeroRho);

            AddLog10Plans(result, _hankel, rhoMin, rhoMax, rhoStep, _numberOfHankels);

            return result;
        }

        private static void AddLog10Plans(ScalarPlan plan, HankelCoefficients hankel, double rhoMin, double rhoMax, double rhoStep, int n)
        {
            var rho1 = CreateRhoFromMin(rhoMin, rhoMax, rhoStep);

            var r2 = rho1[rho1.Length - 1];
            var r1 = rho1[rho1.Length - 2];

            var step = (r2 - r1) / n;


            plan.AddNewLog10PlanItem(hankel, rho1);

            for (int i = 1; i < n; i++)
            {
                var rhoN = CreateRhoFromMax(rhoMin, r2 - i * step, rhoStep);
                plan.AddNewLog10PlanItem(hankel, rhoN);
            }
        }

        private static double[] CreateRhoFromMin(double rhoMin, double rhoMax, double rhoStep)
        {
            var result = new List<double>();

            var nextRho = rhoMin;

            while (nextRho <= rhoMax)
            {
                result.Add(nextRho);
                nextRho *= rhoStep;
            }

            result.Add(nextRho);

            return result.ToArray();
        }

        private static double[] CreateRhoFromMax(double rhoMin, double rhoMax, double rhoStep)
        {
            var result = new List<double>();

            var nextRho = rhoMax;

            while (nextRho >= rhoMin)
            {
                result.Add(nextRho);
                nextRho /= rhoStep;
            }

            // conver rhoMax
            result.Add(nextRho);
            result.Reverse();

            return result.ToArray();
        }

        #region Rho Level

        private static double FindRhoMax(LateralDimensions dimensions, IEnumerable<ObservationLevel> levels)
            => levels.Max(level => FindRhoMax(dimensions, level.ShiftAlongX, level.ShiftAlongY));

        private static double FindRhoMin(LateralDimensions dimensions, IEnumerable<ObservationLevel> levels)
            => levels.Min(level => FindRhoMin(dimensions, level.ShiftAlongX, level.ShiftAlongY));

        private static double FindRhoMax(LateralDimensions dimensions, IEnumerable<SourceLayer> levels)
         => levels.Max(level => FindRhoMax(dimensions, level.ShiftAlongX, level.ShiftAlongY));

        private static double FindRhoMin(LateralDimensions dimensions, IEnumerable<SourceLayer> levels)
            => levels.Min(level => FindRhoMin(dimensions, level.ShiftAlongX, level.ShiftAlongY));

        public static double FindRhoMax(LateralDimensions dimensions, SourceLayer src, params ObservationLevel[] levels)
          => levels.Max(level => FindRhoMax(dimensions, src.ShiftAlongX, src.ShiftAlongY, level.ShiftAlongX, level.ShiftAlongY));

        public static double FindRhoMin(LateralDimensions dimensions, SourceLayer src, params ObservationLevel[] levels)
            => levels.Min(level => FindRhoMin(dimensions, src.ShiftAlongX, src.ShiftAlongY, level.ShiftAlongX, level.ShiftAlongY));

        private static double FindRhoMax(LateralDimensions dim, decimal srcX, decimal srcY, decimal shiftX, decimal shiftY)
        {
            double[] rhos = {
             FindRhoMaxByFourPoints(dim, shiftX, shiftY, srcX, srcY),
             FindRhoMaxByFourPoints(dim, shiftX, shiftY, srcX, srcY + dim.CellSizeY * (dim.Ny-1)),
             FindRhoMaxByFourPoints(dim, shiftX, shiftY, srcX+dim.CellSizeX * (dim.Nx-1), srcY + dim.CellSizeY * (dim.Ny-1)),
             FindRhoMaxByFourPoints(dim, shiftX, shiftY, srcX+dim.CellSizeX * (dim.Nx-1), srcY)};

            return rhos.Max();
        }

        public static double FindRhoMax(LateralDimensions dim, decimal shiftX, decimal shiftY)
        {
            double[] rhos = {
             FindRhoMaxByFourPoints(dim, shiftX, shiftY, 0, 0),
             FindRhoMaxByFourPoints(dim, shiftX, shiftY, 0, dim.CellSizeY * (dim.Ny-1)),
             FindRhoMaxByFourPoints(dim, shiftX, shiftY, dim.CellSizeX * (dim.Nx-1), dim.CellSizeY * (dim.Ny-1)),
             FindRhoMaxByFourPoints(dim, shiftX, shiftY, dim.CellSizeX * (dim.Nx-1), 0)};

            return rhos.Max();
        }

        private static double FindRhoMaxByFourPoints(LateralDimensions dim, decimal shiftX, decimal shiftY, decimal x0, decimal y0)
        {
            var x1 = shiftX - dim.CellSizeX / 2;
            var y1 = shiftY - dim.CellSizeY / 2;
            var x2 = x1 + dim.CellSizeX * dim.Nx;
            var y2 = y1 + dim.CellSizeY * dim.Ny;

            double[] rhos = {
             Sqrt((double)((x1 - x0) * (x1 - x0) + (y1 - y0) * (y1 - y0))),
             Sqrt((double)((x2 - x0) * (x2 - x0) + (y1 - y0) * (y1 - y0))),
             Sqrt((double)((x2 - x0) * (x2 - x0) + (y2 - y0) * (y2 - y0))),
             Sqrt((double)((x1 - x0) * (x1 - x0) + (y2 - y0) * (y2 - y0)))};

            return rhos.Max();
        }

        private static double FindRhoMin(LateralDimensions dim, decimal xShift, decimal yShift)
            => Min(FindRhoMinX(dim, xShift), FindRhoMinY(dim, yShift));

        private static double FindRhoMin(LateralDimensions dim, decimal srcX, decimal srcY, decimal xShift, decimal yShift)
          => Min(FindRhoMin(dim.Nx, dim.CellSizeX, xShift, srcX),
                 FindRhoMin(dim.Ny, dim.CellSizeY, yShift, srcY));

        public static double FindRhoMinX(LateralDimensions dim, decimal xShift)
            => FindRhoMin(dim.Nx, dim.CellSizeX, xShift);

        public static double FindRhoMinY(LateralDimensions dim, decimal yShift)
            => FindRhoMin(dim.Ny, dim.CellSizeY, yShift);

        private static double FindRhoMin(int numberOfCells, decimal cellSize, decimal shift, decimal anomalyShift = 0)
        {
            var anomalyStart = anomalyShift - cellSize / 2;
            var anomalyEnd = anomalyShift + cellSize * numberOfCells - cellSize / 2;

            var yStart = shift - cellSize / 2;
            var yEnd = yStart + (cellSize * numberOfCells);

            if (yStart.IsBetween(anomalyStart, anomalyEnd) ||
                yEnd.IsBetween(anomalyStart, anomalyEnd))
            {
                if (Abs(anomalyStart - yStart) % cellSize == 0)
                    return (double)(cellSize / 2);

                var tmp = Abs(cellSize / 2 - Abs(anomalyStart - yStart) % (cellSize));
                return (double)tmp;
            }

            var anomalyLeftCenter = anomalyShift;
            var anomalyRightCenter = anomalyShift + (cellSize * (numberOfCells - 1));

            var rhoY = Min(Min(Abs(yStart - anomalyLeftCenter), Abs(yStart - anomalyRightCenter)),
                           Min(Abs(yEnd - anomalyLeftCenter), Abs(yEnd - anomalyRightCenter)));

            return (double)rhoY;
        }

        #endregion

        #region Rho Min Site
        private static double FindRhoMin(LateralDimensions dimensions, params ObservationSite[] sites)
             => sites.Min(site => FindRhoMin(dimensions, site));

        private static double FindRhoMin(LateralDimensions dimensions, ObservationSite site)
        {
            if (SiteIsInsideAnomaly(dimensions, site))
                return FindRhoMinInsideAnomaly(dimensions, site);

            return FindRhoMinOutsideAnomaly(dimensions, site);
        }

        private static double FindRhoMinOutsideAnomaly(LateralDimensions dimensions, ObservationSite site)
        {
            var rhos = new[]
            {
                Abs(site.X),
                Abs(site.Y),
                Abs(site.X-dimensions.Nx * dimensions.CellSizeX),
                Abs(site.Y-dimensions.Ny * dimensions.CellSizeY),
            };

            return (double)rhos.Min();
        }

        private static double FindRhoMinInsideAnomaly(LateralDimensions dimensions, ObservationSite site)
        {
            var minX = FindMinXInsideAnomaly(dimensions, site);
            var minY = FindMinYInsideAnomaly(dimensions, site);

            return (double)Min(minX, minY);
        }

        private static bool SiteIsInsideAnomaly(LateralDimensions dimensions, ObservationSite site)
        {
            return !(site.X < 0 || site.Y < 0 ||
                     site.X > dimensions.Nx * dimensions.CellSizeX ||
                     site.Y > dimensions.Ny * dimensions.CellSizeY);
        }

        public static decimal FindMinXInsideAnomaly(LateralDimensions dimensions, ObservationSite site)
        {
            var remainder = Decimal.Remainder(site.X, dimensions.CellSizeX);

            return Min(remainder, dimensions.CellSizeX - remainder);
        }

        public static decimal FindMinYInsideAnomaly(LateralDimensions dimensions, ObservationSite site)
        {
            var remainder = Decimal.Remainder(site.Y, dimensions.CellSizeY);

            return Min(remainder, dimensions.CellSizeY - remainder);
        }

        #endregion

        #region Rho Max Site
        private static double FindRhoMax(LateralDimensions dimensions, IReadOnlyCollection<ObservationSite> sites)
            => sites.Max(s => FindRhoMax(dimensions, s));

        private static double FindRhoMax(LateralDimensions dimensions, ObservationSite site)
        {
            if (site == null) throw new ArgumentNullException(nameof(site));

            var anomalySizeX = dimensions.Nx * dimensions.CellSizeX;
            var anomalySizeY = dimensions.Ny * dimensions.CellSizeY;

            var anomalyXStart = 0;
            var anomalyYStart = 0;

            var anomalyXEnd = anomalyXStart + anomalySizeX;
            var anomalyYEnd = anomalyYStart + anomalySizeY;

            var maxX = (double)Max(Abs(anomalyXStart - site.X), Abs(anomalyXEnd - site.X));
            var maxY = (double)Max(Abs(anomalyYStart - site.Y), Abs(anomalyYEnd - site.Y));

            return Sqrt(maxX * maxX + maxY * maxY);
        }
        #endregion

        #region Rho Anomaly

        private static double FindRhoMin(LateralDimensions dimensions)
        {
            var half1 = (double)(dimensions.CellSizeX / 2);
            var half2 = (double)(dimensions.CellSizeY / 2);

            return Min(half1, half2);
        }

        private static double FindRhoMax(LateralDimensions dimensions)
        {
            var half1 = (double)(dimensions.CellSizeX / 2);
            var half2 = (double)(dimensions.CellSizeY / 2);

            var anomalySizeX = (double)(dimensions.Nx * dimensions.CellSizeX);
            var anomalySizeY = (double)(dimensions.Ny * dimensions.CellSizeY);

            var maxX = anomalySizeX - half1;
            var maxY = anomalySizeY - half2;

            return Sqrt(maxX * maxX + maxY * maxY);
        }

        #endregion
    }
}
