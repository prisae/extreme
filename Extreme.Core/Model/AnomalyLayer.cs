//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
﻿using System.Xml.Linq;

namespace Extreme.Core
{
    public class AnomalyLayer : IAnomalyLayer
    {
        public AnomalyLayer(decimal depth, decimal thickness)
        {
            Depth = depth;
            Thickness = thickness;
        }

        public decimal Depth { get; }
        public decimal Thickness { get; }
    }
}
