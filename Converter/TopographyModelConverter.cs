using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
        private readonly IDiscreteTopographyProvider _topography;

        public static double AirConductivity = 1E-7;
        public static double OceanConductivity = 3.2;
        public static double CrustConductivity = 0.01;

        public decimal MinZ { get; set; } = 0;
        public decimal MaxZ { get; set; } = 1000;


        public TopographyModelConverter(IDiscreteTopographyProvider topo, ManualBoundaries mb, ILogger logger = null)
            : base(logger)
        {
            if (topo == null) throw new ArgumentNullException(nameof(topo));

            SetBoundaries(mb.StartX, mb.EndX, mb.StartY, mb.EndY);
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

        protected override void FillSigma(CartesianSection1D section1D, CartesianAnomaly anomaly, LateralDimensions lateral)
        {
            var x0 = (double)StartX;
            var y0 = (double)StartY;

            var xSize = (double)lateral.CellSizeX;
            var ySize = (double)lateral.CellSizeY;

            var zLength = anomaly.Layers.Count();

            for (int i = LocalNxStart; i < LocalNxStart + LocalNxLength; i++)
            {
                Logger.WriteStatus($"nx: {i + 1} of {lateral.Nx}");

                for (int j = 0; j < lateral.Ny; j++)
                {
                    var x = x0 + i * xSize;
                    var y = y0 + j * ySize;

                    var depths = _topography.GetDepths(x, y, xSize, ySize);

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
                        
                        var topCond = z1 > section1D.ZeroAirLevelAlongZ ? OceanConductivity : AirConductivity;

                        anomaly.Sigma[i, j, k] = impact == 0
                            ? topCond
                            : CalculateCunductivity(impact, topCond, CrustConductivity);
                    }
                }
            }
        }

        private double CalculateCunductivity(double impact, double oceanConductivity, double crustConductivity)
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
    }
}

