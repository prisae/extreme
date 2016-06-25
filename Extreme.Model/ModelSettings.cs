//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
﻿using System;

namespace Extreme.Model
{
    public class ModelSettings
    {
        public MeshParameters Mesh { get; }
        public ManualBoundaries ManualBoundaries { get; }

        public ModelSettings(MeshParameters mesh, ManualBoundaries manualBoundaries)
        {
            if (mesh == null) throw new ArgumentNullException(nameof(mesh));
            if (manualBoundaries == null) throw new ArgumentNullException(nameof(manualBoundaries));

            Mesh = mesh;
            ManualBoundaries = manualBoundaries;
        }

        public ModelSettings(MeshParameters mesh)
            : this(mesh, ManualBoundaries.Auto)
        {
        }
    }
}
