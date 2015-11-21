using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Extreme.Core;

namespace Extreme.Cartesian.Project
{
    public class XProjectSerializer
    {
        private const string ObservationsSection = @"Observations";

        private const string ObservationSiteItem = @"S";
        private const string ObservationSiteNameAttr = @"name";
        private const string ObservationSiteXAttr = @"x";
        private const string ObservationSiteYAttr = @"y";
        private const string ObservationSiteZAttr = @"z";

        private const string ObservationLevel = @"L";
        private const string ObservationLevelZCoordinateAttr = @"z";
        private const string ObservationLevelNameAttr = @"name";
        private const string ObservationLevelXShiftAttr = @"xShift";
        private const string ObservationLevelYShiftAttr = @"yShift";

        private const string SourcesSection = @"Sources";

        private const string SourceLayer = @"L";
        private const string SourceLayerZCoordinateAttr = @"z";
        private const string SourceLayerXShiftAttr = @"xShift";
        private const string SourceLayerYShiftAttr = @"yShift";

        #region Save

        public static void Save(XProject project, string fileName)
            => ToXDocument(project).Save(fileName);

        public static XDocument ToXDocument(XProject project)
        {
            return new XDocument(
                new XElement("XProject", new XAttribute("version", 2.0),
                new XElement("ProblemType", ""),
                new XElement("ResultsPath", project.ResultsPath),
                new XElement("ModelFile", project.ModelFile),
                FrequenciesToXElement(project.Frequencies),
                PeriodsToXElement(project.Periods),
                ObservationsToXElement(project.ObservationLevels.ToArray(), project.ObservationSites.ToArray()),
                SourcesToXElement(project.SourceLayers.ToArray()),
                ForwardSettingsToXElement(project.ForwardSettings)));
        }

        private static XElement ForwardSettingsToXElement(ForwardSettings forwardSettings)
        {
            return new XElement("ForwardSettings",
                new XElement("Residual", forwardSettings.Residual),
                new XElement("InnerBufferLength", forwardSettings.InnerBufferLength),
                new XElement("OuterBufferLength", forwardSettings.OuterBufferLength),
                new XElement("MaxRepeatsNumber", forwardSettings.MaxRepeatsNumber),
                new XElement("NumberOfHankels", forwardSettings.NumberOfHankels));
        }

        private static XElement ObservationsToXElement(ObservationLevel[] observationLevels, ObservationSite[] observationSites)
        {
            return new XElement(ObservationsSection,
                observationLevels.Length != 0 ? new XElement("Levels", observationLevels.Select(ToXElement)) : null,
                observationSites.Length != 0 ? new XElement("Sites", observationSites.Select(ToXElement)) : null);
        }

        private static XElement SourcesToXElement(SourceLayer[] sourceLayers)
        {
            return new XElement(SourcesSection,
                sourceLayers.Length != 0 ? new XElement("Layers", sourceLayers.Select(ToXElement)) : null);
        }

        private static XElement ToXElement(ObservationSite site)
        {
            return new XElement(ObservationSiteItem,
               new XAttribute(ObservationSiteNameAttr, site.Name),
               new XAttribute(ObservationSiteXAttr, site.X),
               new XAttribute(ObservationSiteYAttr, site.Y),
               new XAttribute(ObservationSiteZAttr, site.Z));
        }

        private static XElement ToXElement(ObservationLevel level)
        {
            return new XElement(ObservationLevel,
                new XAttribute(ObservationLevelNameAttr, level.Name),
                new XAttribute(ObservationLevelXShiftAttr, level.ShiftAlongX),
                new XAttribute(ObservationLevelYShiftAttr, level.ShiftAlongY),
                new XAttribute(ObservationLevelZCoordinateAttr, level.Z));
        }

        private static XElement ToXElement(SourceLayer layer)
        {
            return new XElement(SourceLayer,
                new XAttribute(SourceLayerXShiftAttr, layer.ShiftAlongX),
                new XAttribute(SourceLayerYShiftAttr, layer.ShiftAlongY),
                new XAttribute(SourceLayerZCoordinateAttr, layer.Z));
        }

        private static XElement PeriodsToXElement(IReadOnlyCollection<double> periods)
            => new XElement("Periods", periods.Select(f => new XElement("P", f)));

        private static XElement FrequenciesToXElement(IReadOnlyCollection<double> frequencies)
            => new XElement("Frequencies", frequencies.Select(f => new XElement("F", f)));

        #endregion

        #region Load

        public static XProject Load(string fileName)
            => FromXDocument(XDocument.Load(fileName));

        public static XProject FromXDocument(XDocument xdoc)
        {
            var xproj = xdoc.Element("XProject");

            var xfreqs = xproj?.Element("Frequencies");
            var freqs = xfreqs?.Elements("F").Select(x => x.ValueAsDouble()).ToArray();

            var xpers = xproj?.Element("Periods");
            var pers = xpers?.Elements("P").Select(x => x.ValueAsDouble()).ToArray();


            var levels = LoadLevelObservations(xproj);
            var sites = LoadSiteObservations(xproj);
            var srcLayers = LoadSourceLayers(xproj);

            var settings = LoadForwardSettings(xproj);

            return XProject.NewWithFrequenciesAndPeriods(freqs, pers)
                .WithForwardSettings(settings)
                .WithSources(srcLayers)
                .WithObservations(levels)
                .WithObservations(sites)
                .WithModelFile(xproj?.Element("ModelFile")?.Value)
                .WithResultsPath(xproj?.Element("ResultsPath")?.Value);
        }

        private static ForwardSettings LoadForwardSettings(XElement xproj)
        {
            var xset = xproj.Element("ForwardSettings");

            return new ForwardSettings()
            {
                InnerBufferLength = xset?.ElementAsIntOrNull("InnerBufferLength") ?? 10,
                OuterBufferLength = xset?.ElementAsIntOrNull("OuterBufferLength") ?? 10,
                MaxRepeatsNumber = xset?.ElementAsIntOrNull("MaxRepeatsNumber") ?? 10,
                NumberOfHankels = xset?.ElementAsIntOrNull("NumberOfHankels") ?? 10,
                Residual = xset?.ElementAsDoubleOrNull("Residual") ?? 1E-10,
            };
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
            var xobs = xproj?.Element(ObservationsSection)?.Element("Levels");

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

            int xShift = xelem.AttributeAsIntOrZero(ObservationLevelXShiftAttr);
            int yShift = xelem.AttributeAsIntOrZero(ObservationLevelYShiftAttr);

            var name = xelem.AttributeAsStringOrEmpty(ObservationLevelNameAttr);

            return new ObservationLevel(xShift, yShift, z, name);
        }

        #endregion
    }
}
