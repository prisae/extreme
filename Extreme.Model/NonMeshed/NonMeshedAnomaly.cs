//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
﻿namespace Extreme.Model
{
    public class NonMeshedAnomaly
    {
        public NonMeshedAnomaly(double conductivity, Direction x, Direction y, Direction z)
        {
            Conductivity = conductivity;
            X = x;
            Y = y;
            Z = z;
        }

        public double Conductivity { get; }
        public Direction X { get; }
        public Direction Y { get; }
        public Direction Z { get; }
    }
}
