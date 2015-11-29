using System.Xml.Linq;

namespace Extreme.Core
{
    public class AnomalyLayer : IAnomalyLayer
    {
        public AnomalyLayer(decimal depth, decimal thickness)
        {
            Depth = depth;
            Thickness = thickness;
        }

        public decimal Depth { get; }
        public decimal Thickness { get; }
    }
}
