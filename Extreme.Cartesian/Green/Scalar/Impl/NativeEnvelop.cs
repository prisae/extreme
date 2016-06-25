//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
﻿using System.Runtime.InteropServices;

namespace Extreme.Cartesian.Green.Scalar.Impl
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct NativeEnvelop
    {
        public int length;
        public int r;
        public int s;
        public int t;
        public int b;

        public double dt;
        public double db1;

        public double r1;
        public double r2;
        public double s1;
        public double s2;
    }
}
