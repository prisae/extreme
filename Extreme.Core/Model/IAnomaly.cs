//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
﻿using System.Collections.ObjectModel;

namespace Extreme.Core
{
    public interface IAnomaly
    {
        /// <summary>
        /// Number of cells along dimension1 and dimention2
        /// nx and ny in cartesian
        /// phi and tetta in global
        /// </summary>
        Size2D LocalSize { get; }
        
        /// <summary>
        /// Anomaly layers
        /// </summary>
        ReadOnlyCollection<IAnomalyLayer> Layers { get; }
    }
}
