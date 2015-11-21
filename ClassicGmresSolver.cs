using System;
using Extreme.Core;

using static System.Numerics.Complex;

namespace Extreme.Fgmres
{
    public unsafe class ClassicGmresSolver : GmresSolver
    {
        public ClassicGmresSolver(ILogger logger, INativeMemoryProvider memoryProvider, GmresParams parameters) :
            base(logger, memoryProvider, parameters)
        {
        }

        protected override void ApplyPreconditionerRight(NativeVector src, NativeVector dst, int jH)
        {
            if (Parameters.Preconditioning != Preconditioning.None)
                throw new NotImplementedException();

            UnsafeNativeMethods.Zcopy(Parameters.LocalDimensionSize, src.Ptr, dst.Ptr);
        }

        protected override void CalculateSolutionProjectionToKrylovSubspace(int jH)
        {
            UnsafeNativeMethods.ZgemvNotTrans(Parameters.LocalDimensionSize, jH, One, _krylovBasis.Ptr, _yCurrent.Ptr, Zero, _xCurrent.Ptr);
            if (Parameters.Preconditioning != Preconditioning.None)
                throw new NotImplementedException();
            UnsafeNativeMethods.Zcopy(Parameters.LocalDimensionSize, _xCurrent.Ptr, _r0.Ptr);
        }
    }
}
