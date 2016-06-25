//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
﻿using Extreme.Cartesian.Project;

namespace Extreme.Cartesian.Forward
{
    public class ForwardSettings : ProjectSettings
    {
        public int MaxRepeatsNumber { get; set; } = 100;
        public double Residual { get; set; } = 1E-8;
        public int NumberOfHankels { get; set; } = 10;
        public int OuterBufferLength { get; set; } = 15;
        public int InnerBufferLength { get; set; } = 10;

        public ForwardSettings() : base("Forward")
        {

        }
    }
}
