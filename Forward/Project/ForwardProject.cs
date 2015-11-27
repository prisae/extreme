using System;
using System.Collections.Generic;
using Extreme.Cartesian.Project;
using Extreme.Core;

namespace Extreme.Cartesian.Forward
{
    public class ForwardProject
    {
        public ExtremeProject Extreme { get; }
        public ForwardSettings ForwardSettings => Extreme.Settings["Forward"] as ForwardSettings;
        public string ResultsPath => Extreme.ResultsPath;
        public string ModelFile => Extreme.ModelFile;
        public IReadOnlyCollection<double> Frequencies => Extreme.Frequencies;
        public ObservationLevel[] ObservationLevels => Extreme.ObservationLevels;
        public ObservationSite[] ObservationSites => Extreme.ObservationSites;

        private ForwardProject(double[] frequencies)
        {
            Extreme = new ExtremeProject(frequencies,
                        new Dictionary<string, ProjectSettings>
                        {
                            ["Forward"] = new ForwardSettings()
                        });
        }

        private ForwardProject(ExtremeProject extremeProject)
        {
            if (extremeProject == null) throw new ArgumentNullException(nameof(extremeProject));
            Extreme = extremeProject;
        }

        public static ForwardProject NewWithFrequencies(params double[] frequencies)
                    => new ForwardProject(frequencies);

        public static ForwardProject NewFrom(ExtremeProject extremeProject)
                  => new ForwardProject(extremeProject);
    }
}
