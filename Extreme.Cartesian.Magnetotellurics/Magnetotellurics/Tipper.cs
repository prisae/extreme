//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
﻿using System.Numerics;
using Extreme.Core;

namespace Porvem.Magnetotellurics
{
    public class Tipper
    {
        public Tipper(Tipper tipper): this(tipper.Zx, tipper.Zy)
        {
        }

        public Tipper(Complex zx, Complex zy)
        {
            Zx = zx;
            Zy = zy;
        }

        public Complex Zx { get; }
        public Complex Zy { get; }
    }
}
