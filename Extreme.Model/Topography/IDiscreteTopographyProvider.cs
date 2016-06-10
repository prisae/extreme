using System.Collections.Generic;

namespace Extreme.Model.Topography
{
    public interface IDiscreteTopographyProvider
    {
        List<double> GetDepths(double x, double y, double xSize, double ySize);
        double GetMinZ();
        double GetMaxZ();
    }
}
