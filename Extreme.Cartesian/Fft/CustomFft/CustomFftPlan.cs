//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
﻿using System;
using System.Numerics;
using Extreme.Cartesian.FftW;

namespace Extreme.Cartesian.Fft
{
    public unsafe class CustomFftPlan
    {
        public int Dimension { get; }
        public UnsafeArray Input { get; }
        public UnsafeArray Output { get; }

        public CustomFftPlan(Complex* inputBuffer, Complex* outputBuffer, int dimension)
        {
            if (dimension != 2 && dimension != 3)
                throw new ArgumentOutOfRangeException(nameof(dimension));

            Dimension = dimension;
            Input = new UnsafeArray(inputBuffer);
            Output = new UnsafeArray(outputBuffer);
        }

        public int LocalNx;
        public int Ny;
        public int Nc;

        public int NumberOfMpiProcesses;
        public int NumberOfLocalFftsAlongSecondDimension;


        public FftwPlan TransposePlan;
        public FftwPlan ForwardX;
        public FftwPlan ForwardY;
        public FftwPlan BackwardX;
        public FftwPlan BackwardY;

        public FftwPlan ForwardZ;
        public FftwPlan BackwardZ;
    }
}
