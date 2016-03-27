using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Extreme.Cartesian.Forward;
using Extreme.Core;

namespace Extreme.Cartesian.Project
{
    public class ProjectReader : ProjectSerializer
    {
        private readonly IDictionary<string, IProjectSettingsReader> _settingsReaders;


        public ProjectReader(IDictionary<string, IProjectSettingsReader> settingsReaders)
        {
            _settingsReaders = settingsReaders;
        }

        public ProjectReader()
            : this(new Dictionary<string, IProjectSettingsReader>())
        {
        }

        public ExtremeProject FromXDocument(XDocument xdoc)
        {
            var xproj = xdoc.Element("ExtremeProject");

            if (xproj == null)
                throw new InvalidDataException("No ExtremeProject section in project file");

            var xfreqs = xproj?.Element("Frequencies");
            var freqs = xfreqs?.Elements("F").Select(x => x.ValueAsDouble()).ToArray();

            var levels = LoadLevelObservations(xproj);
            var sites = LoadSiteObservations(xproj);
            var srcLayers = LoadSourceLayers(xproj);

            var settings = LoadSettings(xproj);

            return new ExtremeProject(freqs, settings)
                .WithSources(srcLayers)
                .WithObservations(levels)
                .WithObservations(sites)
                .WithModelFile(xproj?.Element("ModelFile")?.Value.Trim())
                .WithResultsPath(xproj?.Element("ResultsPath")?.Value.Trim());
        }

        private IDictionary<string, ProjectSettings> LoadSettings(XElement xproj)
        {
            var result = new Dictionary<string, ProjectSettings>();
            var xelem = xproj.Element("Settings");

            foreach (var key in _settingsReaders.Keys)
            {
                var settings = _settingsReaders[key].FromXElement(xelem?.Element(key));
                result.Add(key, settings);
            }

            return result;
        }

        private static ObservationSite[] LoadSiteObservations(XElement xproj)
        {
            var xobs = xproj?.Element(ObservationsSection)?.Element("Sites");

            if (xobs == null)
                return new ObservationSite[0];

            var observationPoints = xobs.Elements(ObservationSiteItem).Select(xelem =>
            {
                var name = xelem.AttributeAsStringOrEmpty(ObservationSiteNameAttr);

                var x = xelem.AttributeAsDecimal(ObservationSiteXAttr);
                var y = xelem.AttributeAsDecimal(ObservationSiteYAttr);
                var z = xelem.AttributeAsDecimal(ObservationSiteZAttr);

                return new ObservationSite(x, y, z, name);
            });

            return observationPoints.ToArray();
        }

        private static SourceLayer[] LoadSourceLayers(XElement xproj)
        {
            var xobs = xproj?.Element(SourcesSection)?.Element("Layers");

            if (xobs == null)
                return new SourceLayer[0];

            return xobs.Elements(SourceLayer)
                    .Select(XElementToSourceLayers).ToArray();
        }

        private static ObservationLevel[] LoadLevelObservations(XElement xproj)
        {
            var xobs = xproj?.Element(ObservationsSection)?.Element("Tablets");

            if (xobs == null)
                return new ObservationLevel[0];

            return xobs.Elements(ObservationLevel)
                    .Select(XElementToObservationLevel).ToArray();
        }

        private static SourceLayer XElementToSourceLayers(XElement xelem)
        {
            var z = xelem.AttributeAsDecimal(SourceLayerZCoordinateAttr);

            int xShift = xelem.AttributeAsIntOrZero(SourceLayerXShiftAttr);
            int yShift = xelem.AttributeAsIntOrZero(SourceLayerYShiftAttr);

            return new SourceLayer(xShift, yShift, z);
        }

        private static ObservationLevel XElementToObservationLevel(XElement xelem)
        {
            var z = xelem.AttributeAsDecimal(ObservationLevelZCoordinateAttr);

            decimal xShift = xelem.AttributeAsDecimalOrZero(ObservationLevelXShiftAttr);
			decimal yShift = xelem.AttributeAsDecimalOrZero(ObservationLevelYShiftAttr);

            var name = xelem.AttributeAsStringOrEmpty(ObservationLevelNameAttr);

            return new ObservationLevel(xShift, yShift, z, name);
        }
    }
}
