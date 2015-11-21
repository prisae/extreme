using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using Extreme.Cartesian.Model;
using Extreme.Core;

namespace Porvem.Cartesian.Magnetotellurics
{
    public class ResultsContainer : IDisposable
    {
        public Dictionary<double, List<AllFieldsAtLevel>> LevelFields { get; }
        public LateralDimensions Lateral { get; }

        public ResultsContainer(LateralDimensions lateral)
        {
            Lateral = lateral;
            LevelFields = new Dictionary<double, List<AllFieldsAtLevel>>();
        }

        public void Add(double frequency, AllFieldsAtLevel levelField)
        {
            if (!LevelFields.ContainsKey(frequency))
                LevelFields.Add(frequency, new List<AllFieldsAtLevel>());

            LevelFields[frequency].Add(levelField);
        }

        public void Dispose()
        {
            foreach (var value in LevelFields.Values)
                foreach (var atLevel in value)
                {
                    atLevel.NormalE1.Dispose();
                    atLevel.NormalE2.Dispose();
                    atLevel.NormalH1.Dispose();
                    atLevel.NormalH2.Dispose();

                    atLevel.AnomalyE1.Dispose();
                    atLevel.AnomalyE2.Dispose();
                    atLevel.AnomalyH1.Dispose();
                    atLevel.AnomalyH2.Dispose();
                }
        }

        //     public void SaveToFile(string fileName) => SaveToXDocument().Save(fileName);

        public static ResultsContainer Load(string fileName, LateralDimensions lateral = default(LateralDimensions))
            => Load(XDocument.Load(fileName), lateral);

        public static ResultsContainer Load(XDocument xdoc, LateralDimensions lateral = default(LateralDimensions))
        {
            var xresult = xdoc.Element("ResultsMT");

            if (xresult == null)
                throw new ArgumentOutOfRangeException("xdoc");

            if (lateral == default(LateralDimensions))
                lateral = ModelLoadSerializer.LateralDimensionsFromXElement(xresult);

            if (lateral == default(LateralDimensions))
                throw new InvalidOperationException("no lateral dimensions");

            var xfreq = xresult.Element("Frequencies");
            var xobs = xresult.Element("Observations");
            var xvals = xresult.Element("Values");

            if (xfreq == null) throw new InvalidDataException("Frequencies");
            if (xobs == null) throw new InvalidDataException("Observations");
            if (xvals == null) throw new InvalidDataException("Values");

            var result = new ResultsContainer(lateral);

            return result;
        }


    }
}
