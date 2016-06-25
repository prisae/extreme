//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
﻿using System.Xml.Linq;
using Extreme.Core;

namespace Extreme.Cartesian.Model
{
    public class CartesianAnomalyLayer: AnomalyLayer
    {
        public CartesianAnomalyLayer(decimal depth, decimal thickness)
            :base (depth, thickness)
        {
        }

        public XElement UnderlyingXml { get; set; }
    }
}
