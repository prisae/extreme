using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Extreme.Core;

namespace Extreme.Cartesian.Project
{
    public class ProjectWriter :ProjectSerializer
    {
        private readonly ExtremeProject _project;
        private readonly IDictionary<string, IProjectSettingsWriter> _settingsWriters;

     
        public ProjectWriter(ExtremeProject project, IDictionary<string, IProjectSettingsWriter> settingsWriters)
        {
            if (project == null) throw new ArgumentNullException(nameof(project));
            if (settingsWriters == null) throw new ArgumentNullException(nameof(settingsWriters));

            _project = project;
            _settingsWriters = settingsWriters;
        }

        public ProjectWriter(ExtremeProject project)
            :this (project, new Dictionary<string, IProjectSettingsWriter>())
        {
        }

        public XDocument ToXDocument()
        {
            return new XDocument(
                new XElement("ExtremeProject", new XAttribute("version", CurrentVersion),
                    new XElement("ProblemType", ""),
                    new XElement("ResultsPath", _project.ResultsPath),
                    new XElement("ModelFile", _project.ModelFile),
                    FrequenciesToXElement(_project.Frequencies),
                    ObservationsToXElement(_project.ObservationLevels.ToArray(), _project.ObservationSites.ToArray()),
                    SourcesToXElement(_project.SourceLayers.ToArray()),
                    SettingsToXElement()));
        }

        private XElement SettingsToXElement()
        {
            var xsettings = new XElement("Settings");

            foreach (var settings in _project.Settings)
            {
                if (_settingsWriters.ContainsKey(settings.Key))
                    xsettings.Add(_settingsWriters[settings.Key].ToXElement(settings.Value)); 
            }

            return xsettings;
        }


        private static XElement ObservationsToXElement(ObservationLevel[] observationLevels, ObservationSite[] observationSites)
        {
            return new XElement(ObservationsSection,
                observationLevels.Length != 0 ? new XElement("Tablets", observationLevels.Select(ToXElement)) : null,
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

        private static XElement FrequenciesToXElement(IReadOnlyCollection<double> frequencies)
            => new XElement("Frequencies", frequencies.Select(f => new XElement("F", f)));
    }
}
