//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
﻿using System.Collections.Generic;

namespace Extreme.Model.Topography
{
    public interface IDiscreteTopographyProvider
    {
        List<double> GetDepths(double x, double y, double xSize, double ySize);
        double GetMinZ();
        double GetMaxZ();
    }
}
