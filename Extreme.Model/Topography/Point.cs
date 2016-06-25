//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
﻿namespace Extreme.Model.Topography
{
    public struct Point
    {
        public double X { get; private set; }
        public double Y { get; private set; }
        public double Z { get; private set; }
        public Point(double x, double y, double z)
            : this()
        {
            X = x;
            Y = y;
            Z = z;
        }
    }
}
