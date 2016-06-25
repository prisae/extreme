//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
﻿namespace Extreme.Core
{
    public class Transmitter : TransceiverElement
    {
        private Transmitter(decimal startingDepth, decimal thickness, int index)
            : base(startingDepth, thickness, index)
        {
        }

        public static Transmitter NewFlat(decimal depth, int correspondingLayerIndex = -1) 
            => new Transmitter(depth, 0, correspondingLayerIndex);

        public static Transmitter NewVolumetric(decimal startingDepth, decimal thickness, int correspondingLayerIndex = -1) 
            => new Transmitter(startingDepth, thickness, correspondingLayerIndex);
    }
}
