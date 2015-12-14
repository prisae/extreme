using System;
using System.Collections.Generic;
using System.Linq;
using Extreme.Cartesian.Model;
using ModelCreaters;

namespace Extreme.Model
{
    public class AnalyticToCartesianModelConverter : ToCartesianModelConverter
    {
        private readonly AnalyticModel _analyticModel;
        private decimal _zStart;
        private decimal _zSize;

        public AnalyticToCartesianModelConverter(AnalyticModel analyticModel, ManualBoundaries mb)
        {
            _analyticModel = analyticModel;

            SetBoundaries(mb.StartX, mb.EndX, mb.StartY, mb.EndY);
        }

        protected override decimal[] CreateAnomalyFragmentation(MeshParameters mesh)
        {
            return CreateAnomalyFragmentation(mesh, _analyticModel.MinZ, _analyticModel.MaxZ);
        }

        protected override bool CheckSimpleGriddingPossibility(decimal xCellSize, decimal yCellSize)
        {
            return true;
        }

        protected override CartesianSection1D ConvertSection1D()
        {
            return _analyticModel.Section1D;
        }

        protected override double GetValueFor(decimal xStart, decimal xSize, decimal yStart, decimal ySize, double backgroundConductivity)
        {

            int nx = 10;
            int ny = 10;

            var xStep = xSize / nx;
            var yStep = ySize / ny;

            var values = new List<double>();


            for (int i = 0; i < nx; i++)
            {
                for (int j = 0; j < ny; j++)
                {
                    var x = xStart + xStep * (i + 0.5m);
                    var y = yStart + yStep * (j + 0.5m);

                    values.Add(_analyticModel.GetValue(x, y, _zStart + _zSize / 2));
                }
            }

            return values.Average();
        }

        protected override void PrepareLayer(decimal start, decimal end)
        {
            _zStart = start;
            _zSize = end - start;
        }
    }
}
