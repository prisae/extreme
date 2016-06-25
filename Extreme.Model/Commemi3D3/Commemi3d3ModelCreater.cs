//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
﻿using System.Collections.Generic;
using System.Linq;
using Extreme.Cartesian.Model;
using Extreme.Core.Model;

namespace Extreme.Model
{
    public static class Commemi3D3ModelCreater
    {
        public static NonMeshedModel CreateModelOnlyBottomBlock()
        {
            var section1D = CreateSection1D();

            var model = new NonMeshedModel(section1D);
            
            model.AddAnomaly(new NonMeshedAnomaly(conductivity: 1 / 0.3f,
                                        x: new Direction(1400, 1000),
                                        y: new Direction(0, 5600),
                                        z: new Direction(1000, 2000)));

            return model;
        }

        public static NonMeshedModel CreateModelWithFirstLayerBlockOnly()
        {
            var section1D = CreateSection1D();

            var model = new NonMeshedModel(section1D);

            model.AddAnomaly(new NonMeshedAnomaly(conductivity: 1 / 0.1f,
                                       x: new Direction(0, 1000),
                                       y: new Direction(0, 2000),
                                       z: new Direction(200, 800)));

            return model;
        }


        public static NonMeshedModel CreateNonMeshedModel()
        {
            var section1D = CreateSection1D();

            var model = new NonMeshedModel(section1D);

            model.AddAnomaly(new NonMeshedAnomaly(conductivity: 1 / 300f,
                                        x: new Direction(0, 3000),
                                        y: new Direction(400, 400),
                                        z: new Direction(50, 250)));

            model.AddAnomaly(new NonMeshedAnomaly(conductivity: 1 / 30f,
                                        x: new Direction(0, 3000),
                                        y: new Direction(800, 600),
                                        z: new Direction(50, 400)));

            model.AddAnomaly(new NonMeshedAnomaly(conductivity: 1 / 100f,
                                        x: new Direction(0, 3000),
                                        y: new Direction(1400, 400),
                                        z: new Direction(50, 250)));
            
            model.AddAnomaly(new NonMeshedAnomaly(conductivity: 1 / 30f,
                                        x: new Direction(0, 3000),
                                        y: new Direction(1800, 600),
                                        z: new Direction(50, 400)));
            
            model.AddAnomaly(new NonMeshedAnomaly(conductivity: 1 / 300f,
                                        x: new Direction(0, 3000),
                                        y: new Direction(2400, 400),
                                        z: new Direction(50, 250)));

            model.AddAnomaly(new NonMeshedAnomaly(conductivity: 1 / 0.1f,
                                        x: new Direction(3400, 1000),
                                        y: new Direction(2800, 2000),
                                        z: new Direction(200, 800)));

            model.AddAnomaly(new NonMeshedAnomaly(conductivity: 1/0.3f,
                                        x: new Direction(1400, 1000),
                                        y: new Direction(0, 5600),
                                        z: new Direction(1000, 2000)));
            
            return model;
        }



        private static ISection1D<Layer1D> CreateSection1D()
        {
            IEnumerable<ResistivityLayer1D> layers = new ResistivityLayer1D[]
            {
                new ResistivityLayer1D(0, float.PositiveInfinity), 
                new ResistivityLayer1D(1000m, 1000), 
                new ResistivityLayer1D(6500m, 10000), 
                new ResistivityLayer1D(0, 10), 
            };


            var section1D = new Section1D<ResistivityLayer1D>(layers.ToArray());

            return section1D;
        }
    }
}
