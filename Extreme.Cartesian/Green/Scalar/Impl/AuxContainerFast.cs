using System;
using System.Numerics;
using Extreme.Cartesian.Model;
using Extreme.Core;

namespace Extreme.Cartesian.Green.Scalar.Impl
{
    public class AuxContainerFast
    {
        public Complex[,] Eta;
        public Complex[,] Exp;
        public Complex[,] P;
        public Complex[,] Q;

        public Complex[, ][] A;

        public readonly Transmitter Tr;
        public readonly Receiver Rc;

        public readonly int CorrBackgroundTr;
        public readonly int CorrBackgroundRc;

        private AuxContainerFast(Transmitter tr, Receiver rc, int corrBackgroundTr, int corrBackgroundRc)
        {
            Tr = tr;
            Rc = rc;
            CorrBackgroundTr = corrBackgroundTr;
            CorrBackgroundRc = corrBackgroundRc;
        }
        
        public static AuxContainerFast CreateContainer(OmegaModel model, Transmitter tr, Receiver rc)
        {
            var corrBackgroundTr = model.DetermineCorrespondingBackgroundLayer(tr.GetWorkingDepth());
            var corrBackgroundRc = model.DetermineCorrespondingBackgroundLayer(rc.GetWorkingDepth());
            
            var c = new AuxContainerFast(tr, rc, corrBackgroundTr, corrBackgroundRc);

            return c;
        }
    }
}
