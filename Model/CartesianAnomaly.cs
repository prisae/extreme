using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Extreme.Core;

namespace Extreme.Cartesian.Model
{
    public class CartesianAnomaly
    {
        public Size2D LocalSize { get; private set; }
        public ReadOnlyCollection<CartesianAnomalyLayer> Layers { get; }

        public CartesianAnomaly(Size2D localSize, IList<CartesianAnomalyLayer> anomalyLayers)
        {
            LocalSize = localSize;
            Layers = new ReadOnlyCollection<CartesianAnomalyLayer>(anomalyLayers);
        }

        public void ChangeLocalSize(Size2D size)
        {
            LocalSize = size;
        }
    }
}
