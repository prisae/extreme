//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
﻿using Extreme.Cartesian;
using Extreme.Core;

namespace Extreme.Cartesian.Magnetotellurics
{
    public class AllFieldsAtSite
    {
        public ObservationSite Site { get; }

        public AllFieldsAtSite(ObservationSite site)
        {
            Site = site;
        }

        public AllFieldsAtSite(AllFieldsAtSite site)
        {
            Site = site.Site;

            NormalE1 = new ComplexVector(site.NormalE1); 
            NormalE2 = new ComplexVector(site.NormalE2);
            NormalH1 = new ComplexVector(site.NormalH1);
            NormalH2 = new ComplexVector(site.NormalH2);

            AnomalyE1 = new ComplexVector(site.AnomalyE1);
            AnomalyE2 = new ComplexVector(site.AnomalyE2);
            AnomalyH1 = new ComplexVector(site.AnomalyH1);
            AnomalyH2 = new ComplexVector(site.AnomalyH2);
        }

        public ComplexVector NormalE1 { get; set; }
        public ComplexVector NormalE2 { get; set; }
        public ComplexVector NormalH1 { get; set; }
        public ComplexVector NormalH2 { get; set; }

        public ComplexVector AnomalyE1 { get; set; }
        public ComplexVector AnomalyE2 { get; set; }
        public ComplexVector AnomalyH1 { get; set; }
        public ComplexVector AnomalyH2 { get; set; }

        public ComplexVector FullE1 => NormalE1 + AnomalyE1;
        public ComplexVector FullE2 => NormalE2 + AnomalyE2;
        public ComplexVector FullH1 => NormalH1 + AnomalyH1;
        public ComplexVector FullH2 => NormalH2 + AnomalyH2;
    }
}
