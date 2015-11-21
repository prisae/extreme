using System;
using Extreme.Core;
using Extreme.Core.Model;

namespace Extreme.Cartesian.Model
{
    public static class ModelUtils
    {
        public static decimal[] GetBackgroundLayerDepths(ISection1D<Layer1D> section1D)
        {
            var backgroundDepths = new decimal[section1D.NumberOfLayers];
            backgroundDepths[0] = section1D.ZeroAirLevelAlongZ;

            for (int i = 0; i < section1D.NumberOfLayers - 1; i++)
                backgroundDepths[i + 1] = backgroundDepths[i] + section1D[i].Thickness;

            return backgroundDepths;
        }

        public static T FindCorrespondingBackgroundLayer<T>(ISection1D<T> section1D, IAnomalyLayer anomalyLayer) where T : Layer1D
        {
            var index = FindCorrespondingBackgroundLayerIndex(section1D, anomalyLayer);
            return section1D[index];
        }

        public static int FindCorrespondingBackgroundLayerIndex(ISection1D<Layer1D> section1D, IAnomalyLayer anomalyLayer)
        {
            var start = anomalyLayer.Depth;
            var end = anomalyLayer.Depth + anomalyLayer.Thickness;

            return FindCorrespondingBackgroundLayerIndex(section1D, start, end);
        }

        public static int FindCorrespondingBackgroundLayerIndex(ISection1D<Layer1D> section1D, decimal start, decimal end)
        {
            decimal ss = section1D.ZeroAirLevelAlongZ;

            for (int i = 0; i < section1D.NumberOfLayers; i++)
            {
                var layer = section1D[i];

                var es = ss + layer.Thickness;

                if (start >= ss && end <= es)
                    return i;

                ss = es;
            }

            throw new InvalidOperationException();
        }
    }
}
