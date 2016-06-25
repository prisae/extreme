//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
﻿namespace Extreme.Core
{
    public class ObservationLevel
    {
        public ObservationLevel(decimal shiftAlongX, decimal shiftAlongY, decimal z, string name = "")
        {
            ShiftAlongX = shiftAlongX;
            ShiftAlongY = shiftAlongY;
            Z = z;
            Name = name;
        }

        public ObservationLevel(decimal z, string name = "")
        {
            Z = z;
            Name = name;
        }

        public string Name { get; }
        public decimal Z { get; }
        public decimal ShiftAlongX { get; } = 0;
        public decimal ShiftAlongY { get; } = 0;
    }
}
