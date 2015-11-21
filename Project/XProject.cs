using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.InteropServices;
using Extreme.Core;

namespace Extreme.Cartesian.Project
{
    public class XProject
    {
        private readonly double[] _frequencies;
        private readonly double[] _periods;

        public IReadOnlyCollection<double> Frequencies => new ReadOnlyCollection<double>(_frequencies);
        public IReadOnlyCollection<double> Periods => new ReadOnlyCollection<double>(_periods);

        public string ResultsPath { get; private set; }
        public string ModelFile { get; private set; }


        public ObservationLevel[] ObservationLevels { get; private set; } = new ObservationLevel[0];
        public ObservationSite[] ObservationSites { get; private set; } = new ObservationSite[0];
        public SourceLayer[] SourceLayers { get; private set; } = new SourceLayer[0];

        public ForwardSettings ForwardSettings { get; private set; } = new ForwardSettings();

        public static XProject NewWithFrequencies(params double[] frequencies)
            => new XProject(frequencies, new double[0]);

        public static XProject NewWithPeriods(params double[] periods)
            => new XProject(new double[0], periods);

        public static XProject NewWithFrequenciesAndPeriods(double[] frequencies, double[] periods) =>
         new XProject(frequencies, periods);

        private XProject(double[] frequencies, double[] periods)
        {
            if (frequencies == null) throw new ArgumentNullException(nameof(frequencies));
            if (periods == null) throw new ArgumentNullException(nameof(periods));

            _frequencies = frequencies;
            _periods = periods;

            ResultsPath = @"results";
        }

        public XProject WithResultsPath(string resultsPath)
        {
            if (resultsPath == null) throw new ArgumentNullException(nameof(resultsPath));
            ResultsPath = resultsPath;
            return this;
        }

        public XProject WithModelFile(string modelFile)
        {
            if (modelFile == null) throw new ArgumentNullException(nameof(modelFile));
            ModelFile = modelFile;
            return this;
        }

        public XProject WithObservations(params ObservationLevel[] levels)
        {
            if (levels == null) throw new ArgumentNullException(nameof(levels));
            ObservationLevels = levels;
            return this;
        }

        public XProject WithSources(params SourceLayer[] layers)
        {
            if (layers == null) throw new ArgumentNullException(nameof(layers));
            SourceLayers = layers;
            return this;
        }

        public XProject WithObservations(params ObservationSite[] sites)
        {
            if (sites == null) throw new ArgumentNullException(nameof(sites));
            ObservationSites = sites;
            return this;
        }

        public XProject WithForwardSettings(ForwardSettings settings)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            ForwardSettings = settings;
            return this;
        }
    }
}
