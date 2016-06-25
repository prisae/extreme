//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
﻿using System.Xml.Linq;

namespace Extreme.Cartesian.Project
{
    public interface IProjectSettingsWriter
    {
        XElement ToXElement(ProjectSettings settings);
    }
}
