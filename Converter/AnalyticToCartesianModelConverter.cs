using System;
using System.Collections.Generic;
using System.Linq;
using Extreme.Cartesian.Model;
using Extreme.Core;
using Extreme.Core.Logger;
using ModelCreaters;

namespace Extreme.Model
{
    public class AnalyticToCartesianModelConverter : ToCartesianModelConverter
    {
        private readonly AnalyticModel _analyticModel;
        private readonly ILogger _logger;
        private decimal _zStart;
        private decimal _zSize;

        public int IntegrateStepsAlongX { get; set; } = 1;
        public int IntegrateStepsAlongY { get; set; } = 1;
        public int IntegrateStepsAlongZ { get; set; } = 1;

        public AnalyticToCartesianModelConverter(AnalyticModel analyticModel, ManualBoundaries mb, ILogger logger = null)
        {
            if (analyticModel == null) throw new ArgumentNullException(nameof(analyticModel));

            _analyticModel = analyticModel;

            _logger = logger;

            SetBoundaries(mb.StartX, mb.EndX, mb.StartY, mb.EndY);
        }

        protected override decimal GetMinZ()
            => _analyticModel.MinZ;

        protected override decimal GetMaxZ()
            => _analyticModel.MaxZ;

        protected override bool CheckSimpleGriddingPossibility(decimal xCellSize, decimal yCellSize)
            => true;

        protected override CartesianSection1D GetSection1D()
            => _analyticModel.Section1D;

        protected override double GetValueFor(decimal xStart, decimal xSize, decimal yStart, decimal ySize, double backgroundConductivity)
        {
            var xStep = xSize / IntegrateStepsAlongX;
            var yStep = ySize / IntegrateStepsAlongY;
            var zStep = _zSize / IntegrateStepsAlongZ;

            var values = new List<double>();

            for (int i = 0; i < IntegrateStepsAlongX; i++)
                for (int j = 0; j < IntegrateStepsAlongY; j++)
                    for (int k = 0; k < IntegrateStepsAlongZ; k++)
                    {
                        var x = xStart + xStep * (i + 0.5m);
                        var y = yStart + yStep * (j + 0.5m);
                        var z = _zStart + zStep * (k + 0.5m);

                        values.Add(_analyticModel.GetValue(x, y, z));
                    }

            return values.Average();
        }

        protected override void PrepareLayer(decimal start, decimal end)
        {
            _logger?.WriteStatus($"Prepare layer {start} {end}");
            _zStart = start;
            _zSize = end - start;
        }
    }
}
