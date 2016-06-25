//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
﻿using Extreme.Cartesian.Model;
using Extreme.Core.Model;

namespace Extreme.Model
{
    public class NaserModelSettings : ModelSettings
    {
        public double TopConductivity { get; private set; } = 0.002;
        public double LeftConductivity { get; private set; } = 1;
        public double RightConductivity { get; private set; } = 0.002;
        
        public NaserModelSettings(MeshParameters mesh, ManualBoundaries manualBoundaries)
            : base(mesh, manualBoundaries)
        {
        }

        public NaserModelSettings(MeshParameters mesh) : base(mesh)
        {
        }

        public NaserModelSettings WithTopConductivity(double sigma)
        {
            TopConductivity = sigma;
            return this;
        }

        public NaserModelSettings WithLeftConductivity(double sigma)
        {
            LeftConductivity = sigma;
            return this;
        }

        public NaserModelSettings WithRightConductivity(double sigma)
        {
            RightConductivity = sigma;
            return this;
        }
    }
}
