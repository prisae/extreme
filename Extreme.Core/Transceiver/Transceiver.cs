//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
﻿using System;

namespace Extreme.Core
{
    public class Transceiver
    {
        public static Transceiver Zero { get; } = new Transceiver(Transmitter.NewFlat(0), Receiver.NewFlat(0));

        public Receiver Receiver { get; }
        public Transmitter Transmitter { get; }

        public Transceiver(Transmitter transmitter, Receiver receiver)
        {
            if (transmitter == null)  throw new ArgumentNullException(nameof(transmitter));
            if (receiver == null)  throw new ArgumentNullException(nameof(receiver));
            
            Receiver = receiver;
            Transmitter = transmitter;
        }
    }
}
