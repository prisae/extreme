using Extreme.Cartesian.Model;
using Extreme.Core.Model;

namespace Extreme.Model
{
    public class NaserModelCreater
    {
        private readonly NaserModelSettings _settings;

        public NaserModelCreater(NaserModelSettings settings)
        {
            _settings = settings;
        }

        public NonMeshedModel CreateNonMeshedModel()
        {
            var section1D = CreateSection1D();

            var model = new NonMeshedModel(section1D);

            model.AddAnomaly(new NonMeshedAnomaly(conductivity: _settings.LeftConductivity,
                                        x: new Direction(0, 7000),
                                        y: new Direction(0, 4000),
                                        z: new Direction(1500, 4500)));

            model.AddAnomaly(new NonMeshedAnomaly(conductivity: _settings.RightConductivity,
                                        x: new Direction(0, 7000),
                                        y: new Direction(4000, 4000),
                                        z: new Direction(1500, 4500)));

            model.AddAnomaly(new NonMeshedAnomaly(conductivity: _settings.TopConductivity,
                                    x: new Direction(2000, 3000),
                                    y: new Direction(2000, 4000),
                                    z: new Direction(200, 300)));

            return model;
        }

        private static ISection1D<Layer1D> CreateSection1D()
        {
            var layers = new[]
            {
                Sigma1DLayer.Air(),
                Sigma1DLayer.Layer(10000, 1e-1),
                Sigma1DLayer.HalfSpace(1e-1),
            };

            return new Section1D<Sigma1DLayer>(layers);
        }
    }
}
