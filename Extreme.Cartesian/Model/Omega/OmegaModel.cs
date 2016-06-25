//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
﻿using System.Collections.ObjectModel;
using Extreme.Core;
using Extreme.Core.Model;

namespace Extreme.Cartesian.Model
{
    public class OmegaModel
    {
        public OmegaAnomaly Anomaly { get; }
        public Section1D<IsotropyLayer> Section1D { get; }
        public LateralDimensions LateralDimensions { get; }
        public double Omega { get; }
        public int Nx => LateralDimensions.Nx;
        public int Ny => LateralDimensions.Ny;
        public int Nz => Anomaly.Layers.Count;

        private readonly decimal[] _backgroundDepths;

        public OmegaModel(CartesianModel model, Section1D<IsotropyLayer> section1D, OmegaAnomaly anomaly, double omega)
        {
            LateralDimensions = model.LateralDimensions;
            Section1D = section1D;
            Anomaly = anomaly;
            Omega = omega;

            _backgroundDepths = new decimal[section1D.NumberOfLayers];
            _backgroundDepths[0] = Section1D.ZeroAirLevelAlongZ;

            for (int i = 0; i < section1D.NumberOfLayers - 1; i++)
                _backgroundDepths[i + 1] = _backgroundDepths[i] + section1D[i].Thickness;
        }

  
        public decimal GetLayerDepth(int index)
        {
            return _backgroundDepths[index];
        }
    }
}
