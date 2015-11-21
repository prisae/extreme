namespace Extreme.Cartesian.Model
{
    public class ModelSerializer
    {
        public const string SecondVersion = @"2.0";
        
        public const string Model = @"CartesianModel";
        public const string ModelVersionAttr = @"version";
        
        public const string LateralDimensionsItem = @"LateralDimensions";
        public const string LateralDimensionsDxAttr = @"dx";
        public const string LateralDimensionsDyAttr = @"dy";
        public const string LateralDimensionsNxAttr = @"nx";
        public const string LateralDimensionsNyAttr = @"ny";
        
        public const string BackgroundSection = @"Background1DSection";
        public const string BackgroundZeroLevelAttr = @"zeroLevel";
        public const string BackgroundLayer = @"Layer";
        public const string BackgroundLayerThicknessAttr = @"thickness";
        public const string BackgroundLayerSigmaRealAttr = @"sigma";
        
        public const string AnomalySection = @"Anomaly";
        public const string AnomalyLayer = @"AnomalyLayer";
        
        public const string AnomalyFromFile = @"FromFile";
        public const string AnomalyApplique = @"Applique";
        public const string AnomalyFileName = @"fileName";
        public const string AnomalyFileType = @"fileType";
        public const string AnomalyLayerDepthAttr = @"depth";
        public const string AnomalyLayerThicknessAttr = @"thickness";
    }
}
