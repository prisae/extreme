using System;

namespace Extreme.Core.Model
{
    public class Section1D<T> : ISection1D<T> where T : Layer1D
    {
        private readonly T[] _layers;

        public Section1D(T[] layers)
            : this(0, layers)
        {
        }

        public Section1D(decimal zeroAirLevel, T[] layers)
        {
            if (layers == null)
                throw new ArgumentNullException(nameof(layers));

            ZeroAirLevelAlongZ = zeroAirLevel;
            _layers = layers;
        }

        public int NumberOfLayers => _layers.Length;

        public decimal ZeroAirLevelAlongZ { get; }

        public T this[int layerIndex] => _layers[layerIndex];
    }
}