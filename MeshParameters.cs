using System;
using System.Collections.Generic;
using Extreme.Core.Model;

namespace Extreme.Model
{
    public class MeshParameters
    {
        public int Nx { get; private set; }
        public int Ny { get; private set; }
        public int Nz { get; private set; }

        private readonly decimal[] _zLevels = new decimal[0];

        public MeshParameters(int nx, int ny, int nz)
        {
            if (nx <= 0) throw new ArgumentOutOfRangeException();
            if (ny <= 0) throw new ArgumentOutOfRangeException();
            if (nz <= 0) throw new ArgumentOutOfRangeException();

            Nx = nx;
            Ny = ny;
            Nz = nz;

            GeometricRation = 1.5;
        }

        public MeshParameters(int nx, int ny, params decimal[] zLevels)
        {
            if (nx <= 0) throw new ArgumentOutOfRangeException();
            if (ny <= 0) throw new ArgumentOutOfRangeException();

            Nx = nx;
            Ny = ny;
            Nz = zLevels.Length;
            _zLevels = zLevels;

            GeometricRation = 1.5;
        }


        public bool UseGeometricStepAlongZ { get; set; }
        public double GeometricRation { get; set; }
        public bool HasPredefinedAnomalyFragmentation
            => _zLevels.Length != 0;


        public decimal[] GetPredefinedAnomalyFragmentation()
        {
            return _zLevels;
        }

        public decimal[] CreateAnomalyFragmentation(NonMeshedModel model)
        {
            return CreateAnomalyFragmentation(model.GetMinZ(), model.GetMaxZ());
        }

        public decimal[] CreateAnomalyFragmentation(decimal minZ, decimal maxZ)
        {
            var anomalyFragmentation = new List<decimal>();

            if (UseGeometricStepAlongZ)
            {
                var z0 = minZ;
                anomalyFragmentation.Add(z0);

                var thick0 = (double)(maxZ - minZ) * (GeometricRation - 1.0) / (Math.Pow(GeometricRation, Nz) - 1);

                decimal nextZ = Math.Floor((decimal)((double)z0 + thick0));
                anomalyFragmentation.Add(nextZ);

                for (int i = 1; i < Nz - 1; i++)
                {
                    var thick = thick0 * Math.Pow(GeometricRation, i);

                    nextZ = Math.Floor((decimal)((double)nextZ + thick));
                    anomalyFragmentation.Add(nextZ);
                }

                anomalyFragmentation.Add(maxZ);
            }
            else
            {
                decimal zCellSize = (maxZ - minZ) / Nz;

                for (decimal z = minZ; z <= maxZ; z += zCellSize)
                    anomalyFragmentation.Add(z);
            }

            return anomalyFragmentation.ToArray();
        }
    }
}
