using System;

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
