using System;
using System.Numerics;
using Extreme.Core;

namespace Extreme.Cartesian.Model
{
    public class OmegaAnomalyLayer : IAnomalyLayer
    {
        private readonly Complex[,] _zeta;

        public OmegaAnomalyLayer(Complex[,] zeta, decimal depth, decimal thickness)
        {
            _zeta = zeta;
            Depth = depth;
            Thickness = thickness;
        }

        public decimal Depth { get; private set; }
        public decimal Thickness { get; private set; }

        public Complex GetZeta(int i, int j)
        {
            return _zeta[i, j];
        }
    }
}
