using System;
using System.Collections.Generic;
using Extreme.Cartesian.Model;
using Extreme.Core;
using Extreme.Model;

namespace ModelCreaters
{
    public abstract class ToCartesianModelConverter
    {
        private int _localNxStart;
        private int _localNxLength;

        protected decimal StartX { get; private set; }
        protected decimal EndX { get; private set; }
        protected decimal StartY { get; private set; }
        protected decimal EndY { get; private set; }

        private bool _createSigma = true;

        protected void SetBoundaries(decimal startX, decimal endX, decimal startY, decimal endY)
        {
            StartX = startX;
            EndX = endX;
            StartY = startY;
            EndY = endY;
        }

        public CartesianModel Convert(MeshParameters meshParameters)
            => Convert(meshParameters, 0, meshParameters.Nx);

        public CartesianModel Convert(MeshParameters meshParameters, int localNxStart, int localNxLength)
            => Convert(meshParameters, localNxStart, localNxLength, createSigma: true);

        public CartesianModel ConvertWithoutAnomalyData(MeshParameters meshParameters)
           => Convert(meshParameters, 0, meshParameters.Nx, createSigma: false);

        private CartesianModel Convert(MeshParameters meshParameters, int localNxStart, int localNxLength, bool createSigma)
        {
            _createSigma = createSigma;

            _localNxStart = localNxStart;
            _localNxLength = localNxLength;

            var anomalyFragmentation = meshParameters.HasPredefinedAnomalyFragmentation ?
                meshParameters.GetPredefinedAnomalyFragmentation() :
                CreateAnomalyFragmentation(meshParameters, GetMinZ(), GetMaxZ());

            return Convert(meshParameters, anomalyFragmentation);
        }

        private CartesianModel Convert(MeshParameters mesh, decimal[] anomalyZSegmentation)
        {
            var xCellSize = (EndX - StartX) / mesh.Nx;
            var yCellSize = (EndY - StartY) / mesh.Ny;

            if (!CheckSimpleGriddingPossibility(xCellSize, yCellSize))
                throw new InvalidOperationException("!CheckSimpleGriddingPossibility(xCellSize, yCellSize)");

            var lateral = new LateralDimensions(mesh.Nx, mesh.Ny, xCellSize, yCellSize);

            var section1D = GetSection1D();
            var anomaly = ConvertAnomaly(lateral, section1D, anomalyZSegmentation);

            return new CartesianModel(lateral, section1D, anomaly);
        }

        protected abstract decimal GetMinZ();
        protected abstract decimal GetMaxZ();
        protected abstract bool CheckSimpleGriddingPossibility(decimal xCellSize, decimal yCellSize);
        protected abstract CartesianSection1D GetSection1D();
        protected abstract double GetValueFor(decimal xStart, decimal xSize, decimal yStart, decimal ySize,
            double backgroundConductivity);

        protected abstract void PrepareLayer(decimal start, decimal end);

        private decimal[] CreateAnomalyFragmentation(MeshParameters mesh, decimal minZ, decimal maxZ)
        {
            var anomalyFragmentation = new List<decimal>();

            if (mesh.UseGeometricStepAlongZ)
            {
                var z0 = minZ;
                anomalyFragmentation.Add(z0);

                var thick0 = (double)(maxZ - minZ) * (mesh.GeometricRation - 1.0) / (Math.Pow(mesh.GeometricRation, mesh.Nz) - 1);

                decimal nextZ = Math.Floor((decimal)((double)z0 + thick0));
                anomalyFragmentation.Add(nextZ);

                for (int i = 1; i < mesh.Nz - 1; i++)
                {
                    var thick = thick0 * Math.Pow(mesh.GeometricRation, i);

                    nextZ = Math.Floor((decimal)((double)nextZ + thick));
                    anomalyFragmentation.Add(nextZ);
                }

                anomalyFragmentation.Add(maxZ);
            }
            else
            {
                decimal zCellSize = (maxZ - minZ) / mesh.Nz;

                for (decimal z = minZ; z <= maxZ; z += zCellSize)
                    anomalyFragmentation.Add(z);
            }

            return anomalyFragmentation.ToArray();
        }


        private CartesianAnomaly ConvertAnomaly(LateralDimensions lateral, CartesianSection1D section1D, decimal[] anomalyZSegmentation)
        {
            var allLayers = new CartesianAnomalyLayer[anomalyZSegmentation.Length - 1];

            for (int k = 0; k < allLayers.Length; k++)
            {
                decimal zStart = anomalyZSegmentation[k];
                decimal zEnd = anomalyZSegmentation[k + 1];

                var thickness = zEnd - zStart;

                allLayers[k] = new CartesianAnomalyLayer(zStart, thickness);
            }

            var anomaly = new CartesianAnomaly(new Size2D(_localNxLength, lateral.Ny), allLayers);

            if (_createSigma)
            {
                anomaly.CreateSigma();

                for (int k = 0; k < allLayers.Length; k++)
                {
                    var layer = allLayers[k];
                    decimal zStart = layer.Depth;
                    decimal zEnd = layer.Depth + layer.Thickness;

                    var index = ModelUtils.FindCorrespondingBackgroundLayerIndex(section1D, layer);
                    var value = section1D[index].Sigma;

                    PrepareLayer(zStart, zEnd);
                    FillLateralGriddingFor(anomaly.Sigma, k, lateral, value);
                }
            }

            return anomaly;
        }

        private void FillLateralGriddingFor(double[,,] sigma, int k, LateralDimensions lateral, double layer1DValue)
        {
            decimal x0 = StartX;
            decimal y0 = StartY;

            for (int i = _localNxStart; i < _localNxStart + _localNxLength; i++)
                for (int j = 0; j < lateral.Ny; j++)
                {
                    var xStart = x0 + i * lateral.CellSizeX;
                    var yStart = y0 + j * lateral.CellSizeY;

                    sigma[i - _localNxStart, j, k]
                        = GetValueFor(xStart, lateral.CellSizeX,
                                      yStart, lateral.CellSizeY,
                                      layer1DValue);
                }
        }
    }
}
