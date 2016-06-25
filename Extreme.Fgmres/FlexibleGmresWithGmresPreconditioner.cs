//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
﻿using System;
using Extreme.Core;


namespace Extreme.Fgmres
{
    public class FlexibleGmresWithGmresPreconditioner: IDisposable
    {
        private readonly FlexibleGmresSolver _flexibleGmres;
        private readonly ClassicGmresSolver _classicGmres;

        public event EventHandler<GmresIterationCompleteEventArgs> IterationComplete;
        private void OnIterationComplete(GmresIterationCompleteEventArgs e)
            => IterationComplete?.Invoke(this, e);

        public event EventHandler<GmresIterationCompleteEventArgs> InnerIterationComplete;
        private void OnInnerIterationComplete(GmresIterationCompleteEventArgs e)
            => InnerIterationComplete?.Invoke(this, e);

        public event EventHandler<MatrixVectorMultRequestEventArgs> MatrixVectorMultRequest;
        private void OnMatrixVectorMultRequest(MatrixVectorMultRequestEventArgs e)
            => MatrixVectorMultRequest?.Invoke(this, e);

        public event EventHandler<DotProductRequestEventArgs> DotProductRequest;
        private void OnDotProductRequest(DotProductRequestEventArgs e)
            => DotProductRequest?.Invoke(this, e);

        public FlexibleGmresWithGmresPreconditioner(
            ILogger logger,
            INativeMemoryProvider memoryProvider,
            GmresParams fgmresParams,
            int innerBufferLength)
        {
            var innerParams = new GmresParams(fgmresParams.LocalDimensionSize, innerBufferLength)
            {
                Tolerance = fgmresParams.Tolerance,
                MaxRepeatNumber = 1,
                ResidualAtRestart = fgmresParams.ResidualAtRestart,
                GramSchmidtType = fgmresParams.GramSchmidtType,
                InitialGuess = InitialGuess.UserSupplied,
                Preconditioning = Preconditioning.None,
                BackwardErrorChecking = BackwardErrorChecking.CheckArnoldiOnly
            };

            _classicGmres = new ClassicGmresSolver(logger, memoryProvider, innerParams);
            _flexibleGmres = new FlexibleGmresSolver(logger, memoryProvider, fgmresParams);

            _classicGmres.DotProductRequest += (s, a) => OnDotProductRequest(a);
            _flexibleGmres.DotProductRequest += (s, a) => OnDotProductRequest(a);
            _classicGmres.MatrixVectorMultRequest += (s, a) => OnMatrixVectorMultRequest(a);
            _flexibleGmres.MatrixVectorMultRequest += (s, a) => OnMatrixVectorMultRequest(a);

            _classicGmres.IterationComplete += (s, a) => OnInnerIterationComplete(a);
            _flexibleGmres.IterationComplete += (s, a) => OnIterationComplete(a);

            _flexibleGmres.RightPreconditionerRequest += (s, a) => _classicGmres.Solve(a.Src, a.Src, a.Dst);
        }

        public ResultInfo Solve(NativeVector rightHandSide, NativeVector initialGuess, NativeVector result)
        {
            return _flexibleGmres.Solve(rightHandSide, initialGuess, result);
        }

        public void Dispose()
        {
            _classicGmres.Dispose();
            _flexibleGmres.Dispose();
        }
    }
}
