using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using Extreme.Core;
using Extreme.Core.Logger;
using static System.Math;
using static System.Numerics.Complex;

namespace Extreme.Fgmres
{
    public abstract unsafe class GmresSolver : IDisposable
    {
        private const int NumberOfIMGSIterations = 3;

        protected GmresParams Parameters { get; }

        protected readonly ILogger Logger;
        protected readonly NativeVector _xCurrent;
        protected readonly NativeVector _yCurrent;
        protected readonly FortranMatrix _krylovBasis;
        protected readonly NativeVector _r0;

        private readonly int _nloc;
        private readonly int _m;

        private readonly NativeVector _dotProducts;
        private readonly NativeVector _w;

        private readonly Complex[] _givensRotSin;
        private readonly double[] _givensRotCos;
        private readonly FortranMatrix _hessenberg;


        private int _iterationCounter = 0;

        protected GmresSolver(ILogger logger, INativeMemoryProvider memoryProvider, GmresParams parameters)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            if (memoryProvider == null) throw new ArgumentNullException(nameof(memoryProvider));

            Logger = logger;
            Parameters = parameters;
            _nloc = Parameters.LocalDimensionSize;
            _m = Parameters.BufferSize;

            _dotProducts = Parameters.GramSchmidtType == GramSchmidtType.IterativeClassical ?
                new NativeVector(memoryProvider, _m) :
                new NativeVector(memoryProvider, 1);

            var krylovSize = Parameters.ResidualAtRestart == ResidualAtRestart.ComputeTheTrue ? _m : _m + 1;
            _krylovBasis = new FortranMatrix(memoryProvider, _nloc, krylovSize);
            _hessenberg = new FortranMatrix(memoryProvider, _m + 1, _m + 1);

            _w = new NativeVector(memoryProvider, _nloc);
            _r0 = new NativeVector(memoryProvider, _nloc);
            _xCurrent = new NativeVector(memoryProvider, _nloc);
            _yCurrent = new NativeVector(memoryProvider, _m);

            _givensRotSin = new Complex[_m];
            _givensRotCos = new double[_m];
        }

        public event EventHandler<GmresIterationCompleteEventArgs> IterationComplete;
        private void OnIterationComplete(GmresIterationCompleteEventArgs e)
            => IterationComplete?.Invoke(this, e);

        public event EventHandler<MatrixVectorMultRequestEventArgs> MatrixVectorMultRequest;
        private void OnMatrixVectorMultRequest(MatrixVectorMultRequestEventArgs e)
            => MatrixVectorMultRequest?.Invoke(this, e);

        public event EventHandler<DotProductRequestEventArgs> DotProductRequest;
        private void OnDotProductRequest(DotProductRequestEventArgs e)
            => DotProductRequest?.Invoke(this, e);


        public ResultInfo Solve(NativeVector rightHandSide, NativeVector initialGuess, NativeVector result)
        {
            _iterationCounter = 0;
            var bn = CalculateVectorNorm(rightHandSide);

            if (CheckRightHandSideForZero(bn))
                return ResultInfo.Converged(0, 0, 0);

            var sb = CalculateScaleFactorsForBackwardErrorUnprecond(bn);
            var sPb = CalculateScaleFactorsForBackwardErrorPrecond(bn);

            if (Parameters.InitialGuess == InitialGuess.EqualsZero)
            {
                initialGuess.SetAllValuesToZero();
                for (int i = 0; i < _r0.Length; i++)
                    _r0[i] = rightHandSide[i];
            }
            else
            {
                CalculateMatrixVectorMult(initialGuess, _r0);
                for (int i = 0; i < _r0.Length; i++)
                    _r0[i] = rightHandSide[i] - _r0[i];
            }


            ApplyPreconditionerLeft(_r0, _w);
            var beta = CalculateVectorNorm(_w);
            CheckForExactSolution(beta);

            double bea = -1;
            double be = -1;
            int iter = 0;

            UnsafeNativeMethods.Zcopy(initialGuess.Length, initialGuess.Ptr, result.Ptr);

            while (true)
            {
                NativeVector.Mult(1 / beta, _w, _krylovBasis.GetColumnVector(0));

                _hessenberg[0, _m] = beta;
                for (int i = 1; i <= _m; i++)
                    _hessenberg[i, _m] = Zero;

                int jH = 0;
                bea = KrylovSubspaceConstruction(bn, ref jH);

                if (bea < Parameters.Tolerance)
                {
                    be = FinalActions(jH, sPb, rightHandSide, result);
                    return ResultInfo.Converged(bea, be, _iterationCounter);
                }

                iter++;


                if (iter >= Parameters.MaxRepeatNumber)
                    break;

                beta = PrepareRestart(rightHandSide, result, jH);
            }

            be = FinalActions(_m, sPb, rightHandSide, result);//ConstructCurrentSolution(_m, result);
            return ResultInfo.NonConverged(bea, be, _iterationCounter);
        }

        private double PrepareRestart(NativeVector rhs, NativeVector x, int jH)
        {
            ConstructCurrentSolution(jH, x);

            double beta;
            if (Parameters.ResidualAtRestart == ResidualAtRestart.ComputeTheTrue)
            {
                UnsafeNativeMethods.Zcopy(_nloc, x.Ptr, _w.Ptr);
                CalculateMatrixVectorMult(_w, _r0);
                NativeVector.Sub(rhs, _r0, _r0);

                if (Parameters.Preconditioning != Preconditioning.None)
                    throw new NotImplementedException();


                UnsafeNativeMethods.Zcopy(_nloc, _r0.Ptr, _w.Ptr);
                beta = CalculateVectorNorm(_w);
            }
            else
            {
                beta = _hessenberg[_m, _m].Magnitude;
                for (int j = _m - 1; j >= 0; j--)
                {
                    _hessenberg[j, _m] = Zero;
                    Zrot(_hessenberg.GetPointer(j, _m), _hessenberg.GetPointer(j + 1, _m), _givensRotCos[j], -_givensRotSin[j]);
                    UnsafeNativeMethods.ZgemvNotTrans(_nloc, _m + 1, One, _krylovBasis.Ptr, _hessenberg.GetColumn(_m), Zero, _w.Ptr);
                }
            }

            return beta;
        }

        private double FinalActions(int jH, double sPb, NativeVector rhs, NativeVector result)
        {
            ConstructCurrentSolution(jH, result);

            if (Parameters.BackwardErrorChecking != BackwardErrorChecking.CheckArnoldiOnly)
                return CheckBackwardError(sPb, rhs);

            return -1;
        }

        private void ConstructCurrentSolution(int jH, NativeVector result)
        {
            UnsafeNativeMethods.Zcopy(jH, _hessenberg.GetColumn(_m), _yCurrent.Ptr);
            UnsafeNativeMethods.SimplifiedZtrsv(jH, _hessenberg.Ptr, _m + 1, _yCurrent.Ptr);

            CalculateSolutionProjectionToKrylovSubspace(jH);

            NativeVector.Add(result, _r0, _xCurrent);
            UnsafeNativeMethods.Zcopy(_nloc, _xCurrent.Ptr, result.Ptr);
        }

        protected abstract void CalculateSolutionProjectionToKrylovSubspace(int jH);

        private double CheckBackwardError(double sPb, NativeVector rhs)
        {
            UnsafeNativeMethods.Zcopy(_nloc, _xCurrent.Ptr, _r0.Ptr);

            CalculateMatrixVectorMult(_r0, _w);

            for (int i = 0; i < _w.Length; i++)
                _w[i] = rhs[i] - _w[i];

            var trueNormRes = CalculateVectorNorm(_w);

            if (Parameters.Preconditioning != Preconditioning.None)
                throw new NotImplementedException();

            var dnormres = trueNormRes;

            var be = dnormres / sPb;

            if (be >= Parameters.Tolerance)
            {
                var messageString = $"be = {be}, tolerance = {Parameters.Tolerance}";

                if (Parameters.BackwardErrorChecking == BackwardErrorChecking.CheckWithException)
                    throw new BackwardErrorException(messageString);

                if (Parameters.BackwardErrorChecking == BackwardErrorChecking.CheckWithWarning)
                    Logger.WriteWarning(messageString);
            }

            return be;
        }


        private double KrylovSubspaceConstruction(double bn, ref int jH)
        {
            var bea = -1.0;

            do
            {
                ApplyPreconditionerRight(_krylovBasis.GetColumnVector(jH), _w, jH);
                CalculateMatrixVectorMult(_w, _r0);
                ApplyPreconditionerLeft(_r0, _w);

                for (int j = 0; j <= jH; j++)
                    _hessenberg[j, jH] = 0;

                int nOrtho = 0;
                double dloo = 0;
                double dnormw;

                do
                {
                    nOrtho++;

                    if (Parameters.GramSchmidtType == GramSchmidtType.IterativeModified)
                        dloo = ModifiedGramSchmidtIteration(jH);
                    else
                        dloo = ClassicalGramSchmidtIteration(jH);

                    dnormw = CalculateVectorNorm(_w);

                } while (ContinueGramSchmidIteration(dnormw, dloo, nOrtho));

                _hessenberg[jH + 1, jH] = dnormw;

                if (jH < _m - 1 || Parameters.ResidualAtRestart == ResidualAtRestart.UseRecurrenceFormula)
                {
                    for (int i = 0; i < _w.Length; i++)
                        _krylovBasis[i, jH + 1] = _w[i] / dnormw;
                }

                for (int j = 0; j < jH; j++)
                    Zrot(_hessenberg.GetPointer(j, jH), _hessenberg.GetPointer(j + 1, jH), _givensRotCos[j], _givensRotSin[j]);


                Zrotg(_hessenberg[jH, jH], _hessenberg[jH + 1, jH], jH);
                Zrot(_hessenberg.GetPointer(jH, _m), _hessenberg.GetPointer(jH + 1, _m), _givensRotCos[jH], _givensRotSin[jH]);
                Zrot(_hessenberg.GetPointer(jH, jH), _hessenberg.GetPointer(jH + 1, jH), _givensRotCos[jH], _givensRotSin[jH]);

                _hessenberg[jH + 1, jH] = Zero;
                var dnormres = Abs(_hessenberg[jH + 1, _m]);

                if (Parameters.Preconditioning != Preconditioning.None)
                    throw new NotImplementedException();

                bea = dnormres / bn;

                _iterationCounter++;
                OnIterationComplete(new GmresIterationCompleteEventArgs(_iterationCounter, bea));

                jH++;

            } while (bea > Parameters.Tolerance && jH < _m);

            return bea;
        }

        private bool ContinueGramSchmidIteration(double dnormw, double dloo, int nOrtho)
        {
            return ((2.0 * dnormw <= Sqrt(dloo)) && (nOrtho < NumberOfIMGSIterations));
        }

        private double ClassicalGramSchmidtIteration(int jH)
        {
            CalculateDotProduct(_krylovBasis.Ptr, _w.Ptr, jH + 1);

            UnsafeNativeMethods.Zaxpy(jH + 1, One, _dotProducts.Ptr, 1, _hessenberg.GetColumn(jH), 1);
            UnsafeNativeMethods.ZgemvNotTrans(_nloc, jH + 1, -One, _krylovBasis.Ptr, _dotProducts.Ptr, One, _w.Ptr);

            var dloo = UnsafeNativeMethods.Dznrm2(jH + 1, _dotProducts.Ptr);
            dloo *= dloo;

            return dloo;
        }


        private double ModifiedGramSchmidtIteration(int jH)
        {
            double dloo = 0;
            for (int j = 0; j <= jH; j++)
            {
                CalculateDotProduct(_krylovBasis.GetColumn(j), _w.Ptr);
                var dVi = _dotProducts[0];

                _hessenberg[j, jH] += dVi;
                dloo += Abs(dVi) * Abs(dVi);
                UnsafeNativeMethods.Zaxpy(_nloc, -dVi, _krylovBasis.GetColumn(j), 1, _w.Ptr, 1);
            }

            return dloo;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Zrot(Complex* x, Complex* y, double c, Complex s)
        {
            UnsafeNativeMethods.Zrot(1, x, 1, y, 1, c, s);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Zrotg(Complex x, Complex y, int jH)
        {
            double c;
            Complex s;

            UnsafeNativeMethods.Zrotg(x, y, &c, &s);

            _givensRotCos[jH] = c;
            _givensRotSin[jH] = s;
        }

        private void CheckForExactSolution(double beta)
        {
            if (beta == 0)
                throw new InvalidOperationException("Why initial guess is exact solution?");
        }

        private double CalculateVectorNorm(NativeVector vector)
        {
            CalculateDotProduct(vector.Ptr, vector.Ptr);

            var vectorNorm = Sqrt(_dotProducts[0].Real);
            return vectorNorm;
        }

        private void ApplyPreconditionerLeft(NativeVector x, NativeVector result)
        {
            if (Parameters.Preconditioning != Preconditioning.None)
                throw new NotImplementedException();

            UnsafeNativeMethods.Zcopy(_nloc, x.Ptr, result.Ptr);
        }

        protected abstract void ApplyPreconditionerRight(NativeVector src, NativeVector dst, int jH);

        private double CalculateScaleFactorsForBackwardErrorPrecond(double bn)
        {
            if (Parameters.Preconditioning != Preconditioning.None)
                throw new NotImplementedException();

            return bn;
        }

        private double CalculateScaleFactorsForBackwardErrorUnprecond(double bn)
        {
            return bn;
        }

        private bool CheckRightHandSideForZero(double bn)
        {
            return (bn == 0);
            //throw new InvalidOperationException("Null right hand side");
        }

        private void CalculateDotProduct(Complex* x, Complex* y, int numberOfDotProducts = 1)
        {
            var args = new DotProductRequestEventArgs(_nloc, x, y, _dotProducts.Ptr, numberOfDotProducts);
            OnDotProductRequest(args);
        }

        private void CalculateMatrixVectorMult(NativeVector x, NativeVector result)
        {
            var args = new MatrixVectorMultRequestEventArgs(x, result);
            OnMatrixVectorMultRequest(args);
        }

        public virtual void Dispose()
        {
            _hessenberg.Dispose();
            _krylovBasis.Dispose();
            _dotProducts.Dispose();
            _w.Dispose();
            _r0.Dispose();
            _xCurrent.Dispose();
            _yCurrent.Dispose();
        }
    }
}
