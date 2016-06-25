//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
﻿using Extreme.Cartesian.Model;
using Extreme.Core.Model;

namespace Extreme.Model
{
    public static class JennekeModelCreater
    {
        private const decimal CoverThickness = 2000m;
        private const decimal OverallFualtThickness = 16000m;

        public static NonMeshedModel CreateModel(decimal lateralSize, decimal faultSize)
        {
            var section1D = CreateSection1D();

            var model = new NonMeshedModel(section1D);

            model.AddAnomaly(new NonMeshedAnomaly(conductivity: 1,
                                        x: new Direction(0, lateralSize),
                                        y: new Direction(0, lateralSize),
                                        z: new Direction(0, CoverThickness)));

            if (faultSize != 0)
            {
                model.AddAnomaly(new NonMeshedAnomaly(conductivity: 1f,
                    x: new Direction(0, faultSize),
                    y: new Direction(0, lateralSize),
                    z: new Direction(CoverThickness, OverallFualtThickness - CoverThickness)));
            }

            return model;
        }

        private static ISection1D<Layer1D> CreateSection1D()
        {
            var layers = new Sigma1DLayer[]
            {
                new Sigma1DLayer(0, 0), 
                new Sigma1DLayer(16000m, 0.01), 
                new Sigma1DLayer(0, 0.1), 
            };

            var section1D = new Section1D<Sigma1DLayer>(layers);

            return section1D;
        }
    }
}
