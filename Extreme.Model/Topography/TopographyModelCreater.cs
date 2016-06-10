using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Extreme.Cartesian.Logger;
using Extreme.Cartesian.Model;
using Extreme.Core;
using Extreme.Core.Model;

namespace Extreme.Model.Topography
{
    public class TopographyModelCreater
    {
        private readonly Point _shift;
        private readonly ILogger _logger;
        private readonly IDiscreteTopographyProvider _topography;

        private readonly decimal _minZ;
        private readonly decimal _maxZ;

        public static float OceanConductivity = 3.2f;
        public static float CrustConductivity = 0.01f;

        public TopographyModelCreater(ILogger logger, IDiscreteTopographyProvider topo, Point shift, double minZ, double maxZ)
        {
            if (logger == null) throw new ArgumentNullException("logger");
            if (topo == null) throw new ArgumentNullException("topo");

            _logger = logger;
            _topography = topo;
            _shift = shift;

            _minZ = (decimal)minZ;
            _maxZ = (decimal)maxZ;
        }

        public CartesianModel CreateModel(MeshParameters mesh, decimal xCellSize, decimal yCellSize)
        {
            var lateral = new LateralDimensions(mesh.Nx, mesh.Ny, xCellSize, yCellSize);
            var section1D = CreateSection1D();
            var anomaly = CreateAnomaly(mesh, lateral);

            var model = new CartesianModel(lateral, section1D, anomaly);


            return model;
        }

        private CartesianAnomaly CreateAnomaly(MeshParameters mesh, LateralDimensions lateral)
        {
            _logger.WriteStatus("\tCreating anomaly ...");

            var size = new Size2D(mesh.Nx, mesh.Ny);
            var layers = new CartesianAnomalyLayer[mesh.Nz];

            var zFragmentation = mesh.CreateAnomalyFragmentation(_minZ, _maxZ);

            WriteZFragmentation(zFragmentation);

            _logger.WriteStatus("\tCreating anomaly layers ...");

            for (int i = 0; i < layers.Length; i++)
                layers[i] = new CartesianAnomalyLayer(zFragmentation[i], zFragmentation[i + 1] - zFragmentation[i]);

            var anomaly = new CartesianAnomaly(size, layers);
            anomaly.CreateSigma();

            _logger.WriteStatus("Anomaly structure created");

            FillModelDueToTopography(lateral, anomaly);

            return anomaly;
        }

        private void WriteZFragmentation(decimal[] zFragmentation)
        {
            _logger.WriteStatus("\tzFragmentation:");

            for (int i = 0; i < zFragmentation.Length; i++)
                _logger.WriteStatus($"{i} {zFragmentation[i]}");
        }

        private CartesianSection1D CreateSection1D()
        {
            return new CartesianSection1D(new[]
            {
                new Sigma1DLayer(0, 0),
                new Sigma1DLayer(_maxZ, 3.2),
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

        private float CalculateCunductivity(double impact, float oceanConductivity, float crustConductivity)
        {
            var value = Math.Exp(Math.Log(oceanConductivity, Math.E) * (1 - impact) + Math.Log(crustConductivity, Math.E) * impact);

            return (float)value;
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
    }
}

