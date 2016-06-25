//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
﻿using Extreme.Cartesian.Model;
using Extreme.Core.Model;

namespace Extreme.Model.SimpleCommemi3D
{
    public class OneBlockModelSettings : ModelSettings
    {
        public CartesianSection1D Section1D { get; set; }

        public decimal AnomalySizeX { get; private set; } = 40000m;
        public decimal AnomalySizeY { get; private set; } = 40000m;
        public decimal AnomalySizeZ { get; private set; } = 10000m;
        public decimal AnomalyStartDepth { get; private set; } = 0m;

        public double Conductivity { get; private set; } = -1;

        public OneBlockModelSettings(MeshParameters mesh, ManualBoundaries manualBoundaries) :
            base(mesh, manualBoundaries)
        {
        }

        public OneBlockModelSettings(MeshParameters mesh)
         : base(mesh)
        {
        }

        public OneBlockModelSettings WithConductivity(double sigma)
        {
            Conductivity = sigma;
            return this;
        }

        public OneBlockModelSettings WithBackgroundConductivity()
        {
            Conductivity = -1;
            return this;
        }

        public OneBlockModelSettings WithAnomalySizeX(decimal x)
        {
            AnomalySizeX = x;
            return this;
        }

        public OneBlockModelSettings WithAnomalySizeY(decimal y)
        {
            AnomalySizeY = y;
            return this;
        }

        public OneBlockModelSettings WithAnomalySizeZ(decimal z)
        {
            AnomalySizeZ = z;
            return this;
        }


        public OneBlockModelSettings WithAnomalyStartDepth(decimal depth)
        {
            AnomalyStartDepth = depth;
            return this;
        }
    }
}