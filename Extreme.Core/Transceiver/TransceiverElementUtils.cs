//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
﻿namespace Extreme.Core
{
    public static class TransceiverElementUtils
    {
        /// <summary>
        /// Checks if this TransceiverElement has a volume
        /// It has a volume when its thickness is greater then zero
        /// </summary>
        /// <param name="transceiverElement"></param>
        /// <returns></returns>
        public static bool HasVolume(this ITransceiverElement transceiverElement)
        {
            return transceiverElement.Thickness > 0;
        }

        /// <summary>
        /// Calculate "working depths" of this TransceiverElement
        /// It equals to StartingDepth + ThicknessInMeters / 2 for volumetric elements,
        /// and StartingDepth for flat elements
        /// </summary>
        /// <param name="transceiverElement"></param>
        /// <returns></returns>
        public static decimal GetWorkingDepth(this ITransceiverElement transceiverElement)
        {
            return transceiverElement.StartingDepth + transceiverElement.Thickness / 2;
        }


        public static decimal GetEndDepth(this ITransceiverElement transceiverElement)
        {
            return transceiverElement.StartingDepth + transceiverElement.Thickness;
        }

    }
}
