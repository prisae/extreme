//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
﻿using Extreme.Core.Model;

namespace Extreme.Cartesian.Model
{
    public class Sigma1DLayer : Layer1D, IConductivityLayer1D
    {
        public static Sigma1DLayer Air()
        {
            return new Sigma1DLayer(0, 0);
        }

        public static Sigma1DLayer HalfSpace(double sigma)
        {
            return new Sigma1DLayer(0, sigma);
        }

        public static Sigma1DLayer Layer(decimal thickness, double sigmaReal)
        {
            return new Sigma1DLayer(thickness, sigmaReal);
        }

        public Sigma1DLayer(decimal thicknessInMeters, double sigma)
            : base(thicknessInMeters)
        {
            Sigma = sigma;
        }

        public double Sigma { get; }
    }
}
