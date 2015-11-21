using System;
using System.Runtime.InteropServices;
using Extreme.Core;
using static System.Numerics.Complex;

namespace Extreme.Fgmres
{
    public unsafe class FlexibleGmresSolver : GmresSolver
    {
        private readonly FortranMatrix _precondKrylovBasis;

        public FlexibleGmresSolver(ILogger logger, INativeMemoryProvider memoryProvider, GmresParams parameters) :
            base(logger, memoryProvider, parameters)
        {
            _precondKrylovBasis = new FortranMatrix(memoryProvider, Parameters.LocalDimensionSize, Parameters.BufferSize);
        }

        public event EventHandler<RightPreconditionerRequestEventArgs> RightPreconditionerRequest;
        private void OnRightPreconditionerRequest(RightPreconditionerRequestEventArgs e)
            => RightPreconditionerRequest?.Invoke(this, e);


        protected override void ApplyPreconditionerRight(NativeVector src, NativeVector dst, int jH)
        {
            var col = _precondKrylovBasis.GetColumnVector(jH);
            OnRightPreconditionerRequest(new RightPreconditionerRequestEventArgs(src, col));
            UnsafeNativeMethods.Zcopy(Parameters.LocalDimensionSize, col.Ptr, dst.Ptr);
        }


        protected override void CalculateSolutionProjectionToKrylovSubspace(int jH)
        {
            UnsafeNativeMethods.ZgemvNotTrans(Parameters.LocalDimensionSize, jH, One, _precondKrylovBasis.Ptr, _yCurrent.Ptr, Zero, _xCurrent.Ptr);
            UnsafeNativeMethods.Zcopy(Parameters.LocalDimensionSize, _xCurrent.Ptr, _r0.Ptr);
        }

        public override void Dispose()
        {
            base.Dispose();
            _precondKrylovBasis.Dispose();
        }
    }
}
