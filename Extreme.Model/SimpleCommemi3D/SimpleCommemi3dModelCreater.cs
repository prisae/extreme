using System.Collections.Generic;
using System.Linq;
using Extreme.Cartesian.Model;
using Extreme.Core.Model;

namespace Extreme.Model
{
    public class SimpleCommemi3DModelCreater
    {
        public decimal AnomalySizeAlongZ { get; } = 10000m;
        public decimal AnomalySizeLateral { get; } = 40000m;

        public SimpleCommemi3DModelCreater()
        {
        }

        public SimpleCommemi3DModelCreater(decimal anomalySizeLateral)
        {
            AnomalySizeLateral = anomalySizeLateral;
        }

        public SimpleCommemi3DModelCreater(decimal anomalySizeAlongZ, decimal anomalySizeLateral)
        {
            AnomalySizeAlongZ = anomalySizeAlongZ;
            AnomalySizeLateral = anomalySizeLateral;
        }

        public NonMeshedModel CreateNonMeshedModel(double leftConductivity, double rightConductivity)
        {
            var section1D = CreateSection1D();

            var model = new NonMeshedModel(section1D);

            model.AddAnomaly(new NonMeshedAnomaly(conductivity: leftConductivity,
                                        x: new Direction(0, AnomalySizeLateral / 2),
                                        y: new Direction(0, AnomalySizeLateral),
                                        z: new Direction(0, AnomalySizeAlongZ)));

            model.AddAnomaly(new NonMeshedAnomaly(conductivity: rightConductivity,
                                        x: new Direction(AnomalySizeLateral / 2, AnomalySizeLateral / 2),
                                        y: new Direction(0, AnomalySizeLateral),
                                        z: new Direction(0, AnomalySizeAlongZ)));


            return model;
        }

        private ISection1D<Layer1D> CreateSection1D()
        {
            IEnumerable<Sigma1DLayer> layers = new[]
            {
                new Sigma1DLayer(0, 0),
                new Sigma1DLayer(200000m, 0),
                new Sigma1DLayer(10000m, 0.1),
                new Sigma1DLayer(20000m, 0.01),
                new Sigma1DLayer(0, 10),
            };

            var section1D = new Section1D<Sigma1DLayer>(-200000m, layers.ToArray());

            return section1D;
        }
    }
}
