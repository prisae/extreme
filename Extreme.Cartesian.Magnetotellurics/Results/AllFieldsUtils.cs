using System;
using Extreme.Cartesian;
using Extreme.Cartesian.Core;
using Extreme.Core;

namespace Extreme.Cartesian.Magnetotellurics
{
    public static class AllFieldsUtils
    {
		public static unsafe AllFieldsAtSite GetSite(this AllFieldsAtLevel level, LateralDimensions lateral, int i, int j,int shiftx=0)
        {
            int ny = lateral.Ny;

            Func<AnomalyCurrent, ComplexVector> get = (f1) => new ComplexVector(
               f1.Ptr[(i * ny + j) * 3],
               f1.Ptr[(i * ny + j) * 3 + 1],
               f1.Ptr[(i * ny + j) * 3 + 2]);

			var x = lateral.CellSizeX * (i+shiftx) + lateral.CellSizeX / 2 + level.Level.ShiftAlongX;
            var y = lateral.CellSizeY * j + lateral.CellSizeY / 2 + level.Level.ShiftAlongY;
            var z = level.Level.Z;
            var name = level.Level.Name;

            return new AllFieldsAtSite(new ObservationSite(x, y, z, name))
            {
                AnomalyE1 = get(level.AnomalyE1),
                AnomalyE2 = get(level.AnomalyE2),
                AnomalyH1 = get(level.AnomalyH1),
                AnomalyH2 = get(level.AnomalyH2),

                NormalE1 = get(level.NormalE1),
                NormalE2 = get(level.NormalE2),
                NormalH1 = get(level.NormalH1),
                NormalH2 = get(level.NormalH2),
            };
        }
    }
}
