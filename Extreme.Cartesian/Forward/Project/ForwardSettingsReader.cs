//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
﻿using System.Xml.Linq;
using Extreme.Cartesian.Project;
using Extreme.Core;

namespace Extreme.Cartesian.Forward
{
    public class ForwardSettingsReader : IProjectSettingsReader
    {
        public ProjectSettings FromXElement(XElement xelem)
        {
            return new ForwardSettings()
            {
                InnerBufferLength = xelem?.ElementAsIntOrNull("InnerBufferLength") ?? 10,
                OuterBufferLength = xelem?.ElementAsIntOrNull("OuterBufferLength") ?? 10,
                MaxRepeatsNumber = xelem?.ElementAsIntOrNull("MaxRepeatsNumber") ?? 10,
                NumberOfHankels = xelem?.ElementAsIntOrNull("NumberOfHankels") ?? 10,
                Residual = xelem?.ElementAsDoubleOrNull("Residual") ?? 1E-10,
            };
        }
    }
}
