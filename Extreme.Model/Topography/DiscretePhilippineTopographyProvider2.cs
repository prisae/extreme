using System.Collections.Generic;

namespace Extreme.Model.Topography
{
    public class DiscretePhilippineTopographyProvider2 : IDiscreteTopographyProvider
    {
        private const int NumberOfX = 5401;
        private const int NumberOfY = 10801;

        private const double XStepAverage = 2.23466897 * 1000;
        private const double YStepAverage = 0.98050175 * 1000;

        private readonly IDiscreteTopographyProvider _provider;

        private DiscretePhilippineTopographyProvider2(IDiscreteTopographyProvider provider)
        {
            _provider = provider;
        }

        public static IDiscreteTopographyProvider CreateProvider(string fileName)
        {
            var loader = new UniformGridTopographyLoader(fileName,
                p => new Point(-p.X * 1000, p.Y * 1000, p.Z));

            var provider = loader.LoadFromBinFile(NumberOfX, NumberOfY, XStepAverage, YStepAverage);
            return new DiscretePhilippineTopographyProvider2(provider);
        }

        public List<double> GetDepths(double x, double y, double xSize, double ySize)
            => _provider.GetDepths(x, y, xSize, ySize);

        public double GetMinZ()
            => _provider.GetMinZ();

        public double GetMaxZ()
            => _provider.GetMaxZ();
    }
}
