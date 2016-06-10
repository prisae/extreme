using System.Collections.Generic;
using System.Linq;
using Extreme.Cartesian.Model;
using Extreme.Core.Model;
using Extreme.Model.SimpleCommemi3D;

namespace Extreme.Model
{
    public class OneBlockModelCreater
    {
        private readonly OneBlockModelSettings _settings;

        public OneBlockModelCreater(OneBlockModelSettings settings)
        {
            _settings = settings;
        }

        public NonMeshedModel CreateNonMeshedModel()
        {
            var section1D = CreateSection1D(_settings.Section1D);

            var model = new NonMeshedModel(section1D);

            var anStart = _settings.AnomalyStartDepth;
            var anEnd = _settings.AnomalyStartDepth + _settings.AnomalySizeZ;

            var start = section1D.ZeroAirLevelAlongZ;

            for (int i = 1; i < section1D.NumberOfLayers - 1; i++)
            {
                var cond = _settings.Conductivity == -1 ?
                    section1D[i].Sigma :
                    _settings.Conductivity;

                var end = start + section1D[i].Thickness;

                if (start < anStart && end > anEnd)
                    AddAnom(model, cond, anStart, anEnd);
                else if (start >= anStart && end > anEnd)
                    AddAnom(model, cond, start, anEnd);
                else if (start >= anStart && end <= anEnd)
                    AddAnom(model, cond, start, end);

                start = end;
            }

            return model;
        }

        private void AddAnom(NonMeshedModel model, double sigma, decimal zStart, decimal zEnd)
        {
            model.AddAnomaly(new NonMeshedAnomaly(sigma,
                     x: new Direction(0, _settings.AnomalySizeX),
                     y: new Direction(0, _settings.AnomalySizeY),
                     z: new Direction(zStart, zEnd - zStart)));
        }

        private ISection1D<Sigma1DLayer> CreateSection1D(Section1D<Sigma1DLayer> section1D)
        {
            var layers = new List<Sigma1DLayer>();

            layers.Add(section1D[0]);

            var newZeroLevel = section1D.ZeroAirLevelAlongZ;

            if (_settings.AnomalyStartDepth < section1D.ZeroAirLevelAlongZ)
            {
                newZeroLevel = _settings.AnomalyStartDepth;
                layers.Add(new Sigma1DLayer(section1D.ZeroAirLevelAlongZ - _settings.AnomalyStartDepth,
                    section1D[0].Sigma));
            }

            for (int i = 1; i < section1D.NumberOfLayers - 1; i++)
                layers.Add(section1D[i]);

            var summ = section1D.ZeroAirLevelAlongZ;
            for (int i = 0; i < section1D.NumberOfLayers; i++)
                summ += section1D[i].Thickness;
            var anomEnd = _settings.AnomalyStartDepth + _settings.AnomalySizeZ;

            if (anomEnd > summ)
                layers.Add(new Sigma1DLayer(anomEnd - summ, section1D[section1D.NumberOfLayers - 1].Sigma));

            layers.Add(section1D[section1D.NumberOfLayers - 1]);

            return new Section1D<Sigma1DLayer>(newZeroLevel, layers.ToArray());
        }
    }
}
