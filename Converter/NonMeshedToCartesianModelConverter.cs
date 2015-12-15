using System;
using System.Collections.Generic;
using System.Linq;
using Extreme.Cartesian.Model;
using Extreme.Core.Model;
using ModelCreaters;

namespace Extreme.Model
{
    public class NonMeshedToCartesianModelConverter : ToCartesianModelConverter
    {
        private readonly NonMeshedModel _model;
        private readonly decimal[] _zBoundaries;

        private List<NonMeshedAnomaly> _currentAnomalies;

        public NonMeshedToCartesianModelConverter(NonMeshedModel model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));

            _model = model;

            _zBoundaries = GetAllBoundariesAlongZ(model);

            var startX = model.Anomalies.Min(a => a.X.Start);
            var endX = model.Anomalies.Max(a => a.X.Start + a.X.Size);
            var startY = model.Anomalies.Min(a => a.Y.Start);
            var endY = model.Anomalies.Max(a => a.Y.Start + a.Y.Size);

            SetBoundaries(startX, endX, startY, endY);
        }

        public void SetManualBoundaries(ManualBoundaries manualBoundaries)
        {
            if (manualBoundaries != ManualBoundaries.Auto)
            {
                SetBoundaries(
                    manualBoundaries.StartX, manualBoundaries.EndX,
                    manualBoundaries.StartY, manualBoundaries.EndY);
            }
        }

        protected override decimal GetMinZ()
            => _model.GetMinZ();

        protected override decimal GetMaxZ()
            => _model.GetMaxZ();

        protected override CartesianSection1D GetSection1D()
        {
            var section1D = _model.Section1D;
            var layers = new List<Sigma1DLayer>();

            for (int i = 0; i < section1D.NumberOfLayers; i++)
                layers.Add(ConvertLayer(section1D[i]));

            return new CartesianSection1D(section1D.ZeroAirLevelAlongZ, layers.ToArray());
        }

        protected override bool CheckSimpleGriddingPossibility(decimal xCellSize, decimal yCellSize)
        {
            bool possibleByX = CheckSimpleGriddingPossibilityByX(_model, xCellSize);
            bool possibleByY = CheckSimpleGriddingPossibilityByY(_model, yCellSize);

            if (!possibleByX)
                return false;

            if (!possibleByY)
                return false;

            return true;
        }

        protected override void PrepareLayer(decimal start, decimal end)
        {
            foreach (var boundary in _zBoundaries)
            {
                if (boundary > start && boundary < end)
                    throw new ArgumentOutOfRangeException($"Z fragmentation is wrong, boundaries are not consistent " +
                                                          $"start:{start}, end:{end}, boundary:{boundary}");
            }

            _currentAnomalies = new List<NonMeshedAnomaly>();

            foreach (var anomaly in _model.Anomalies)
            {
                var z = anomaly.Z;


                if (start >= z.Start && end <= z.Start + z.Size)
                    _currentAnomalies.Add(anomaly);
            }
        }


        protected override double GetValueFor(decimal xStart, decimal xSize, decimal yStart, decimal ySize, double backgroundConductivity)
        {
            if (_currentAnomalies.Count == 0)
                return backgroundConductivity;
            
            var selected = _currentAnomalies.FindAll(a => xStart >= a.X.Start &&
                                                  yStart >= a.Y.Start &&
                                                  xStart + xSize <= a.X.Start + a.X.Size &&
                                                  yStart + ySize <= a.Y.Start + a.Y.Size);

            if (selected.Count > 1)
                throw new InvalidOperationException("Anomalies are intersecting");

            if (selected.Count == 0)
                return backgroundConductivity; ;

            return selected.First().Conductivity;
        }

     
        private Sigma1DLayer ConvertLayer(Layer1D layer)
        {
            double sigma = 0;

            var d = layer as IResistivityLayer1D;
            if (d != null)
                sigma = (1 / d.Resistivity);


            if (layer is IConductivityLayer1D)
                sigma = ((layer as IConductivityLayer1D).Sigma);

            return new Sigma1DLayer(layer.Thickness, sigma);
        }

        private static decimal[] GetAllBoundariesAlongZ(NonMeshedModel model)
        {
            var boundaries = new List<decimal>();

            boundaries.AddRange(GetAllAnomalyBoundariesAlong(a => a.Z, model));
            boundaries.AddRange(GetAllSection1DBoundaries(model));

            return DistinctSortToArray(boundaries);
        }

        private bool CheckSimpleGriddingPossibilityByX(NonMeshedModel model, decimal xCellSize)
        {
            var boundaries = GetAllAnomalyBoundariesAlong(a => a.X, model);
            boundaries.Insert(0, StartX);
            boundaries.Add(EndX);

            return CheckSimpleGriddingPossibility(boundaries, xCellSize);
        }

        private bool CheckSimpleGriddingPossibilityByY(NonMeshedModel model, decimal yCellSize)
        {
            var boundaries = GetAllAnomalyBoundariesAlong(a => a.Y, model);
            boundaries.Add(StartY);
            boundaries.Add(EndY);

            return CheckSimpleGriddingPossibility(boundaries, yCellSize);
        }

        private static bool CheckSimpleGriddingPossibility(List<decimal> boundaries, decimal cellSize)
        {
            if (boundaries.Count == 0)
                return false;

            var start = boundaries[0];

            for (int i = 1; i < boundaries.Count; i++)
            {
                if (Decimal.Remainder(boundaries[i] - start, cellSize) != 0)
                {
                    Console.WriteLine($"{i} {start} {boundaries[i] - start} {cellSize}");

                    return false;
                }
            }

            return true;
        }

        private static decimal[] DistinctSortToArray(IEnumerable<decimal> values)
        {
            var list = values.Distinct().ToList();

            list.Sort();

            return list.ToArray();
        }

        private static IEnumerable<decimal> GetAllSection1DBoundaries(NonMeshedModel model)
        {
            var boundaries = new List<decimal>();
            var backZ = model.Section1D.ZeroAirLevelAlongZ;

            boundaries.Add(backZ);

            for (int i = 0; i < model.Section1D.NumberOfLayers; i++)
            {
                var layer = model.Section1D[i];

                backZ += layer.Thickness;
                boundaries.Add(backZ);
            }

            return boundaries;
        }

        private static List<decimal> GetAllAnomalyBoundariesAlong(Func<NonMeshedAnomaly, Direction> along, NonMeshedModel model)
        {
            var boundaries = new List<decimal>();

            foreach (var anomaly in model.Anomalies)
            {
                var d = along(anomaly);

                boundaries.Add(d.Start);
                boundaries.Add(d.Start + d.Size);
            }

            return boundaries;
        }
    }
}
