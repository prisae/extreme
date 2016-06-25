//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
﻿namespace Extreme.Model.SimpleCommemi3D
{
    public class CommemiModelSettings : ModelSettings
    {
        public int AnomalySizeInMeters { get; private set; }
        public double LeftConductivity { get; private set; }
        public double RightConductivity { get; private set; }

        public CommemiModelSettings(MeshParameters mesh, ManualBoundaries manualBoundaries) : 
            base(mesh, manualBoundaries)
        {
            AnomalySizeInMeters = 40000;

            LeftConductivity = 1;
            RightConductivity = 0.01;
        }

        public CommemiModelSettings(MeshParameters mesh)
            : base(mesh)
        {
            AnomalySizeInMeters = 40000;

            LeftConductivity = 1;
            RightConductivity = 0.01;
        }

        public CommemiModelSettings WithAnomalySizeInMeters(int anomalySizeInMeters)
        {
            AnomalySizeInMeters = anomalySizeInMeters;
            return this;
        }

        public CommemiModelSettings WithConductivities(double left, double right)
        {
            LeftConductivity = left;
            RightConductivity = right;
            return this;
        }
    }
}