using System.Collections.Generic;

namespace Extreme.Model.Topography
{
    public interface IDiscreteTopographyProvider
    {
        List<double> GetDepths(double x, double y, double width, double height);
    }
}
