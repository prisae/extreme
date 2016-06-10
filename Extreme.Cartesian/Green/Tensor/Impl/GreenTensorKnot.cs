﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;

namespace Extreme.Cartesian.Green.Tensor
{
    public struct GreenTensorKnot
    {
        public readonly int Index;
        public readonly double Radius;

        public GreenTensorKnot(int index, double radius)
        {
            Index= index;
            Radius = radius;
        }
    }
}
