//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
﻿using System;
using Extreme.Core;

namespace Extreme.Cartesian.Model
{
    public class CartesianModel
    {
        public int Nx => LateralDimensions.Nx;
        public int Ny => LateralDimensions.Ny;
        public int Nz => Anomaly.Layers.Count;

        public CartesianModel(LateralDimensions lateral, CartesianSection1D section1D, CartesianAnomaly anomaly)
        {
            if (section1D.NumberOfLayers == 0)
                throw new ArgumentOutOfRangeException(nameof(section1D));

            LateralDimensions = lateral;
            Section1D = section1D;
            Anomaly = anomaly;
        }

        public LateralDimensions LateralDimensions { get; }
        public CartesianAnomaly Anomaly { get; }
        public CartesianSection1D Section1D { get; }
    }
}
