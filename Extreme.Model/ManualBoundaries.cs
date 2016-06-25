//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
﻿namespace Extreme.Model
{
    public class ManualBoundaries
    {
        public static ManualBoundaries Auto = new ManualBoundaries(0, 0, 0, 0);

        public decimal StartX { get; private set; }
        public decimal StartY { get; private set; }
        public decimal EndX { get; private set; }
        public decimal EndY { get; private set; }

        public ManualBoundaries(decimal startX, decimal startY, decimal endX, decimal endY)
        {
            StartX = startX;
            StartY = 0;
            StartY = startY;
            EndX = endX;
            EndY = endY;
        }
    }
}
