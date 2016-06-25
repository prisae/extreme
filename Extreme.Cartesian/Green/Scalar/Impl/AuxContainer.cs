//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
﻿using System;
using System.Numerics;
using Extreme.Cartesian.Model;
using Extreme.Core;

namespace Extreme.Cartesian.Green.Scalar.Impl
{
    public class AuxContainer
    {
        public Complex[] Eta;
        public Complex[] Exp;
        public Complex[] P;
        public Complex[] Q;

        public Complex[,] A;

        public readonly Transmitter Tr;
        public readonly Receiver Rc;

        public readonly int CorrBackgroundTr;
        public readonly int CorrBackgroundRc;

        private AuxContainer(Transmitter tr, Receiver rc, int corrBackgroundTr, int corrBackgroundRc)
        {
            Tr = tr;
            Rc = rc;
            CorrBackgroundTr = corrBackgroundTr;
            CorrBackgroundRc = corrBackgroundRc;
        }

        public static AuxContainer CreateContainer(OmegaModel model, Transmitter tr, Receiver rc)
        {
            var corrBackgroundTr = model.DetermineCorrespondingBackgroundLayer(tr.GetWorkingDepth());
            var corrBackgroundRc = model.DetermineCorrespondingBackgroundLayer(rc.GetWorkingDepth());

            var c = new AuxContainer(tr, rc, corrBackgroundTr, corrBackgroundRc);

            return c;
        }
        
        public static AuxContainer CreateContainer(OmegaModel model, decimal trDepth, decimal rcDepth)
        {
            var tr = Transmitter.NewFlat(trDepth);
            var rc = Receiver.NewFlat(rcDepth);

            return CreateContainer(model, tr, rc);
        }
    }
}
