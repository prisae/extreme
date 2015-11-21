﻿using System.Numerics;
using Extreme.Core.Model;

namespace Extreme.Cartesian.Model
{
    public class IsotropyLayer : Layer1D
    {
        public IsotropyLayer(decimal thickness, Complex zeta)
            : base(thickness)
        {
            Zeta = zeta;
        }

        public Complex Zeta { get; }
    }
}
