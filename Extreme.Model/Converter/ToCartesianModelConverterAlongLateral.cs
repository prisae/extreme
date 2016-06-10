using Extreme.Cartesian.Model;
using Extreme.Core;
using ModelCreaters;

namespace Extreme.Model.Converter
{
    public abstract class ToCartesianModelConverterAlongLateral : ToCartesianModelConverter
    {
        protected ToCartesianModelConverterAlongLateral(ILogger logger = null) :
            base(logger)
        {
        }

        protected abstract void PrepareLayer(decimal start, decimal end);
        protected abstract double GetValueFor(decimal xStart, decimal xSize, decimal yStart, decimal ySize,
        double backgroundConductivity);

        protected override void FillSigma(CartesianSection1D section1D, CartesianAnomaly anomaly, LateralDimensions lateral)
        {
            for (int k = 0; k < anomaly.Layers.Count; k++)
            {
                var layer = anomaly.Layers[k];
                decimal zStart = layer.Depth;
                decimal zEnd = layer.Depth + layer.Thickness;

                var index = ModelUtils.FindCorrespondingBackgroundLayerIndex(section1D, layer);
                var value = section1D[index].Sigma;

                PrepareLayer(zStart, zEnd);
                FillLateralGriddingFor(anomaly.Sigma, k, lateral, value);
            }
        }

        private void FillLateralGriddingFor(double[,,] sigma, int k, LateralDimensions lateral, double layer1DValue)
        {
            decimal x0 = StartX;
            decimal y0 = StartY;

            for (int i = LocalNxStart; i < LocalNxStart + LocalNxLength; i++)
                for (int j = 0; j < lateral.Ny; j++)
                {
                    var xStart = x0 + i * lateral.CellSizeX;
                    var yStart = y0 + j * lateral.CellSizeY;

                    sigma[i - LocalNxStart, j, k]
                        = GetValueFor(xStart, lateral.CellSizeX,
                                      yStart, lateral.CellSizeY,
                                      layer1DValue);
                }
        }
    }
}
