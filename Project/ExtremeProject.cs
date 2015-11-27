using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Extreme.Cartesian.Forward;
using Extreme.Core;

namespace Extreme.Cartesian.Project
{
    public class ExtremeProject
    {
        private readonly double[] _frequencies;

        public IReadOnlyCollection<double> Frequencies => new ReadOnlyCollection<double>(_frequencies);

        public string ResultsPath { get; private set; }
        public string ModelFile { get; private set; }


        public ObservationLevel[] ObservationLevels { get; private set; } = new ObservationLevel[0];
        public ObservationSite[] ObservationSites { get; private set; } = new ObservationSite[0];
        public SourceLayer[] SourceLayers { get; private set; } = new SourceLayer[0];

        public IDictionary<string, ProjectSettings> Settings { get; } = new Dictionary<string, ProjectSettings>();

        public ExtremeProject(double[] frequencies)
            : this(frequencies, new Dictionary<string, ProjectSettings>())
        {
        }

        public ExtremeProject(double[] frequencies, IDictionary<string, ProjectSettings> settings)
        {
            if (frequencies == null) throw new ArgumentNullException(nameof(frequencies));
            if (settings == null) throw new ArgumentNullException(nameof(settings));

            _frequencies = frequencies;
            Settings = settings;
            ResultsPath = @"results";
        }

        public ExtremeProject WithResultsPath(string resultsPath)
        {
            if (resultsPath == null) throw new ArgumentNullException(nameof(resultsPath));
            ResultsPath = resultsPath;
            return this;
        }

        public ExtremeProject WithModelFile(string modelFile)
        {
            if (modelFile == null) throw new ArgumentNullException(nameof(modelFile));
            ModelFile = modelFile;
            return this;
        }

        public ExtremeProject WithObservations(params ObservationLevel[] levels)
        {
            if (levels == null) throw new ArgumentNullException(nameof(levels));
            ObservationLevels = levels;
            return this;
        }

        public ExtremeProject WithSources(params SourceLayer[] layers)
        {
            if (layers == null) throw new ArgumentNullException(nameof(layers));
            SourceLayers = layers;
            return this;
        }

        public ExtremeProject WithObservations(params ObservationSite[] sites)
        {
            if (sites == null) throw new ArgumentNullException(nameof(sites));
            ObservationSites = sites;
            return this;
        }
    }
}
