//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Extreme.Core;

namespace Extreme.Cartesian.Model
{
    public class CartesianAnomaly
    {
        public Size2D LocalSize { get; private set; }
        public List<CartesianAnomalyLayer> Layers { get; }

        public double[,,] Sigma { get; private set; }

        public CartesianAnomaly(Size2D localSize, IList<CartesianAnomalyLayer> anomalyLayers)
        {
            LocalSize = localSize;
            Layers = new List<CartesianAnomalyLayer>(anomalyLayers);
        }

        public void CreateSigma()
        {
			Sigma = new double[(long)LocalSize.Nx,(long) LocalSize.Ny, (long)Layers.Count];
        }
        
        public void ChangeLocalSize(Size2D size)
        {
            LocalSize = size;
        }
    }
}
