namespace Extreme.Cartesian.Project
{
    public class ForwardSettings
    {
        public int MaxRepeatsNumber { get; set; } = 100;
        public double Residual { get; set; } = 1E-8;
        public int NumberOfHankels { get; set; } = 10;
        public int OuterBufferLength { get; set; } = 15;
        public int InnerBufferLength { get; set; } = 10;
    }
}
