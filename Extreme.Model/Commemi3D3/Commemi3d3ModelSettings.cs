//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
﻿namespace Extreme.Model
{
    public class Commemi3D3ModelSettings : ModelSettings
    {
        public Commemi3D3ModelSettings(MeshParameters mesh)
            : base(mesh)
        {
        }

        public Commemi3D3ModelSettings(MeshParameters mesh, ManualBoundaries manualBoundaries)
            : base(mesh, manualBoundaries)
        {
        }
    }
}