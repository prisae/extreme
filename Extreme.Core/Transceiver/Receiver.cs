//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
﻿namespace Extreme.Core
{
    public class Receiver : TransceiverElement
    {
        private Receiver(decimal startingDepth, decimal thickness, int index)
            : base(startingDepth, thickness, index)
        {
        }

        public static Receiver NewFlat(decimal depth, int correspondingLayerIndex = -1)
            => new Receiver(depth, 0, correspondingLayerIndex);

        public static Receiver NewVolumetric(decimal startingDepth, decimal thickness, int correspondingLayerIndex = -1)
            => new Receiver(startingDepth, thickness, correspondingLayerIndex);
    }
}
