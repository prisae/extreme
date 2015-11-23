using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Extreme.Core;

namespace Extreme.Cartesian.Model
{
    public class OmegaAnomaly : IAnomaly
    {
        /// <summary>
        /// In distributed systems can be less then full model size.
        /// When local, equals to Model.LeteralDimensions
        /// </summary>
        public Size2D LocalSize { get; }
        public ReadOnlyCollection<IAnomalyLayer> Layers { get; }

        private OmegaAnomaly(Size2D localSize, ReadOnlyCollection<IAnomalyLayer> layers)
        {
            LocalSize = localSize;
            Layers = layers;
        }

        public static OmegaAnomaly CreateFromCartesianAnomaly(double omega, CartesianAnomaly ca)
        {
            var layers = new List<IAnomalyLayer>();

            foreach (var layer in ca.Layers)
            {
                var zeta = OmegaModelUtils.ConvertSigmaToZeta(omega, layer.Sigma);

                layers.Add(new OmegaAnomalyLayer(zeta, layer.Depth, layer.Thickness));
            }

            return new OmegaAnomaly(ca.LocalSize, new ReadOnlyCollection<IAnomalyLayer>(layers));
        }

        public static OmegaAnomaly CreateFromCartesianAnomaly(double omega, CartesianAnomaly ca, double[,,] sigma)
        {
            var zeta = OmegaModelUtils.ConvertSigmaToZeta(omega, sigma);

            var layers = ca.Layers
                .Zip(zeta, (f, s) => new OmegaAnomalyLayer(s, f.Depth, f.Thickness))
                .Cast<IAnomalyLayer>()
                .ToList();

            return new OmegaAnomaly(ca.LocalSize, new ReadOnlyCollection<IAnomalyLayer>(layers));
        }
    }
}
