//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
﻿namespace Extreme.Core
{
    public class SourceLayer
    {
        public decimal Z { get; }
        public decimal ShiftAlongX { get; } = 0;
        public decimal ShiftAlongY { get; } = 0;

        public SourceLayer(decimal shiftAlongX, decimal shiftAlongY, decimal z)
        {
            ShiftAlongX = shiftAlongX;
            ShiftAlongY = shiftAlongY;
            Z = z;
        }
    }
}
