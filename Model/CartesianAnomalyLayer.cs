using System.Numerics;
using System.Xml.Linq;
using Extreme.Core;

namespace Extreme.Cartesian.Model
{
    public class CartesianAnomalyLayer:IAnomalyLayer
    {
        public CartesianAnomalyLayer(Size2D size, decimal depth, decimal thickness)
        {
            Depth = depth;
            Thickness = thickness;
            
            Sigma = new double[size.Nx, size.Ny];
        }

        public CartesianAnomalyLayer(decimal depth, decimal thickness)
        {
            Depth = depth;
            Thickness = thickness;
        }


        public CartesianAnomalyLayer(double[,] sigma, decimal depth, decimal thickness)
        {
            Sigma = sigma;
            Depth = depth;
            Thickness = thickness;
        }

        public double[,] Sigma { get; set; }

        public Complex GetZeta(int i, int j)
            => Sigma[i, j];

        public decimal Depth { get; }
        public decimal Thickness { get; }
        public XElement UnderlyingXml { get; set; }
    }
}
