using System.Xml.Linq;
using Extreme.Core;

namespace Extreme.Cartesian.Model
{
    public class CartesianAnomalyLayer: AnomalyLayer
    {
        public CartesianAnomalyLayer(decimal depth, decimal thickness)
            :base (depth, thickness)
        {
        }

        public XElement UnderlyingXml { get; set; }
    }
}
