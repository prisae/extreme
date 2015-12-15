using System;
using System.Collections.Generic;
using System.Linq;
using Extreme.Cartesian.Logger;
using Extreme.Cartesian.Model;
using Extreme.Core;
using ModelCreaters;

using static System.Math;

namespace Extreme.Model.Topography
{
    public class TopographyModelConverter : ToCartesianModelConverter
    {
        private readonly ILogger _logger;
        private readonly IDiscreteTopographyProvider _topography;


        public static float OceanConductivity = 3.2f;
        public static float CrustConductivity = 0.01f;

        public decimal MinZ { get; set; } = 0;
        public decimal MaxZ { get; set; } = 1000;
        

        public TopographyModelConverter(IDiscreteTopographyProvider topo, ManualBoundaries mb, ILogger logger = null)
        {
            if (topo == null) throw new ArgumentNullException(nameof(topo));

            _logger = logger;
            _topography = topo;
        }


        private CartesianSection1D CreateSection1D()
        {
            return new CartesianSection1D(new[]
            {
                new Sigma1DLayer(0, 0),
                new Sigma1DLayer(MaxZ, 3.2),
                new Sigma1DLayer(0, 0.001),
            });
        }

        private void FillModelDueToTopography(LateralDimensions lateral, CartesianAnomaly anomaly)
        {
            var shift = _shift;

            var xSize = (double)lateral.CellSizeX;
            var ySize = (double)lateral.CellSizeY;

            var zLength = anomaly.Layers.Count();


            System.Threading.Tasks.Parallel.For(0, lateral.Nx, i =>

            //for (int i = 0; i < lateral.Nx; i++)
            {
                _logger.WriteStatus($"nx: {i + 1} of {lateral.Nx}");

                for (int j = 0; j < lateral.Ny; j++)
                {
                    var x = i * xSize;
                    var y = j * ySize;

                    var depths = _topography.GetDepths(x - shift.X, y - shift.Y, xSize, ySize);

                    for (int k = 0; k < zLength; k++)
                    {
                        var z0 = anomaly.Layers[k].Depth;
                        var z1 = z0 + anomaly.Layers[k].Thickness;

                        double impact = CaculateDepthImpact(depths, z0, z1);

                        if (impact == 1)
                        {
                            for (int k2 = k; k2 < zLength; k2++)
                                anomaly.Sigma[i, j, k2] = CrustConductivity;
                            break;
                        }

                        anomaly.Sigma[i, j, k] = impact == 0
                            ? OceanConductivity
                            : CalculateCunductivity(impact, OceanConductivity, CrustConductivity);
                    }
                }
            });
        }

        private double CalculateCunductivity(double impact, float oceanConductivity, float crustConductivity)
        {
            var value = Exp(Log(oceanConductivity, E) * (1 - impact) + Log(crustConductivity, E) * impact);

            return value;
        }

        /// <summary>
        /// 0 - no ground, 1 - full ground
        /// </summary>
        private static double CaculateDepthImpact(List<double> depths, decimal z0Dec, decimal z1Dec)
        {
            var z0 = (double)z0Dec;
            var z1 = (double)z1Dec;

            if (depths.Count > 0)
            {
                double summ = 0;

                for (int i = 0; i < depths.Count; i++)
                {
                    var depth = depths[i];

                    if (depth <= z0)
                        summ += 1;

                    else if (depth < z1)
                        summ += (z1 - depth) / (z1 - z0);
                }

                return (summ / depths.Count);
            }

            return 0;
        }

        protected override decimal GetMinZ() => MinZ;
        protected override decimal GetMaxZ() => MaxZ;
        protected override CartesianSection1D GetSection1D()
            => CreateSection1D();
        protected override bool CheckSimpleGriddingPossibility(decimal xCellSize, decimal yCellSize)
            => true;
        
        protected override double GetValueFor(decimal xStart, decimal xSize, decimal yStart, decimal ySize, double backgroundConductivity)
        {
            throw new NotImplementedException();
        }

        protected override void PrepareLayer(decimal start, decimal end)
        {
            throw new NotImplementedException();
        }
    }
}

