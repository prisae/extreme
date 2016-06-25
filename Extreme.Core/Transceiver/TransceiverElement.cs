//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
﻿namespace Extreme.Core
{
    public abstract class TransceiverElement : ITransceiverElement
    {
        protected TransceiverElement(decimal startingDepth, decimal thickness, int index)
        {
            StartingDepth = startingDepth;
            Thickness = thickness;
            Index = index;
        }

        public decimal StartingDepth { get; }

        public decimal Thickness { get; }

        /// <summary>
        /// Index of corresponding object. 
        /// e.g. index of anomaly layer for anomalies, 
        /// index of observation layer for layered observations
        /// or index of observation site for ObservationSites
        /// </summary>
        public int Index { get; }
    }
}
