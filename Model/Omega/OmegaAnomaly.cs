using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;
using Extreme.Core;

namespace Extreme.Cartesian.Model
{
    public class OmegaAnomaly : IAnomaly
    {
        /// <summary>
        /// In distributed systems can be less then full model size.
        /// When local, equals to Model.LateralDimensions
        /// </summary>
        public Size2D LocalSize { get; }
        public ReadOnlyCollection<IAnomalyLayer> Layers { get; }
        public Complex[,,] Zeta { get; }

        private OmegaAnomaly(Complex[,,] zeta, Size2D localSize, IList<IAnomalyLayer> layers)
        {
            LocalSize = localSize;
            Layers = new ReadOnlyCollection<IAnomalyLayer>(layers);
            Zeta = zeta;
        }

        public static OmegaAnomaly CreateFromCartesianAnomaly(double omega, CartesianAnomaly ca)
        {
            var zeta = OmegaModelUtils.ConvertSigmaToZeta(omega, ca.Sigma);
            var layers = ca.Layers.Select(l => (IAnomalyLayer)l).ToList();
            return new OmegaAnomaly(zeta, ca.LocalSize, layers);
        }

        public static OmegaAnomaly CreateFromCartesianAnomaly(double omega, CartesianAnomaly ca, double[,,] sigma)
        {
            var zeta = OmegaModelUtils.ConvertSigmaToZeta(omega, sigma);
            var layers = ca.Layers.Select(l => (IAnomalyLayer)l).ToList();
            return new OmegaAnomaly(zeta, ca.LocalSize, layers);
        }
    }
}
