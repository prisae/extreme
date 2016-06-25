//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
﻿namespace Extreme.Cartesian.Project
{
    public class ProjectSerializer
    {
        protected const string ObservationsSection = @"Observations";
        
        protected const string ObservationSiteItem = @"S";
        protected const string ObservationSiteNameAttr = @"name";
        protected const string ObservationSiteXAttr = @"x";
        protected const string ObservationSiteYAttr = @"y";
        protected const string ObservationSiteZAttr = @"z";
        
        protected const string ObservationLevel = @"T";
        protected const string ObservationLevelZCoordinateAttr = @"z";
        protected const string ObservationLevelNameAttr = @"name";
        protected const string ObservationLevelXShiftAttr = @"xShift";
        protected const string ObservationLevelYShiftAttr = @"yShift";
        
        protected const string SourcesSection = @"Sources";
        
        protected const string SourceLayer = @"L";
        protected const string SourceLayerZCoordinateAttr = @"z";
        protected const string SourceLayerXShiftAttr = @"xShift";
        protected const string SourceLayerYShiftAttr = @"yShift";
        
        protected const string CurrentVersion = "0.7.2";

        protected ProjectSerializer()
        {
        }
    }
}
