﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extreme.Cartesian.FftW
{
    public class FftwPlan
    {
        public int Nz { get; }
        public int FullNx { get; }
        public int FullNy { get; }
        public IntPtr Handler { get; }

        public FftwPlan(int fullNx, int fullNy, int nz, IntPtr handler)
        {
            Nz = nz;
            FullNx = fullNx;
            FullNy = fullNy;
            Handler = handler;
        }
    }
}
