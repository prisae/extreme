//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
﻿using System;
using Extreme.Cartesian.Core;
using Extreme.Core;

namespace Extreme.Cartesian.Magnetotellurics
{
    public class AllFieldsAtLevel
    {
        public ObservationLevel Level { get; }
        public AllFieldsAtLevel(ObservationLevel level)
        {
            Level = level;
        }
        
        public AnomalyCurrent NormalE1 { get; set; }
        public AnomalyCurrent NormalE2 { get; set; }
        public AnomalyCurrent NormalH1 { get; set; }
        public AnomalyCurrent NormalH2 { get; set; }

        public AnomalyCurrent AnomalyE1 { get; set; }
        public AnomalyCurrent AnomalyE2 { get; set; }
        public AnomalyCurrent AnomalyH1 { get; set; }
        public AnomalyCurrent AnomalyH2 { get; set; }
        
    }
}
