using System;
using System.Numerics;
using System.Runtime.InteropServices;
using Extreme.Cartesian.Core;
using Extreme.Cartesian.Green.Tensor;
using Extreme.Cartesian.Model;
using Extreme.Core;
using Extreme.Core.Model;
using UNM = Extreme.Cartesian.Forward.UnsafeNativeMethods;
using System.Collections.Generic;
using Extreme.Cartesian.Green;
using Extreme.Cartesian.Giem2g;
namespace Extreme.Cartesian.Forward
{
    public unsafe class ConvolutionOperator : ForwardSolverComponent, IDisposable
    {
        private readonly ForwardSolver _solver;

        private readonly Complex* _rFunction;
        private readonly Complex* _forwardFactors;
        private readonly Complex* _backwardFactors;

        private GreenTensor _greenTensor;

        private readonly AnomalyCurrent _rx;

        private AnomalyCurrent _input;
        private AnomalyCurrent _output;
        private OperatorType _operatorType;


        public ConvolutionOperator(ForwardSolver solver)
            : base(solver)
        {
            _solver = solver;

            _rFunction = Alloc(Model.Anomaly.LocalSize.Nx * Model.Anomaly.LocalSize.Ny * Model.Nz);
            _forwardFactors = Alloc(Model.Nz);
            _backwardFactors = Alloc(Model.Nz);

            _rx = AnomalyCurrent.AllocateNewLocalSize(solver.MemoryProvider, solver.Model);
        }

        public void Apply(AnomalyCurrent input, AnomalyCurrent output)
        {
            if (_greenTensor == null) throw new InvalidOperationException($"{nameof(_greenTensor)} is not set");
            if (input == null) throw new ArgumentNullException(nameof(input));
            if (output == null) throw new ArgumentNullException(nameof(output));

            _input = input;
            

			_output = output;

			if (_solver.Engine != ForwardSolverEngine.Giem2g) 
				CalculateOperatorAorKr ();
			else
				DoWithProfiling(ApplyGiem2gOperator, ProfilerEvent.OperatorGiem2gApply);


        }

        public void PrepareOperator(GreenTensor greenTensor, OperatorType operatorType)
        {
            if (greenTensor == null) throw new ArgumentNullException(nameof(greenTensor));
            _greenTensor = greenTensor;
            _operatorType = operatorType;
			if (_solver.Engine != ForwardSolverEngine.Giem2g) {
				var zeta0 = GetBackgroundZeta (Model.Anomaly, Model.Section1D);

				PrepareRFunction (zeta0);
				PrepareBackwardFactors (zeta0);
				PrepareForwardFactors (zeta0);
			} else {
				PrepareGiem2gOperator ();
			}
        }

        private static Complex[] GetBackgroundZeta(IAnomaly anomaly, ISection1D<IsotropyLayer> section1D)
        {
            var zeta0 = new Complex[anomaly.Layers.Count];

            for (int k = 0; k < anomaly.Layers.Count; k++)
                zeta0[k] = ModelUtils.FindCorrespondingBackgroundLayer(section1D, anomaly.Layers[k]).Zeta;

            return zeta0;
        }
        
        private void PrepareForwardFactors(Complex[] zeta0)
        {
            for (int k = 0; k < Model.Nz; k++)
            {
                var value = new Complex(Math.Sqrt(zeta0[k].Real), 0);
                _forwardFactors[k] = value;
            }
        }

        private void PrepareBackwardFactors(Complex[] zeta0)
        {
            var normalizeFactor = new Complex(1 / ((double)Model.Nx * (double)Model.Ny * 4), 0);

            for (int k = 0; k < Model.Nz; k++)
            {
                var scalar = new Complex((2 * Math.Sqrt(zeta0[k].Real)), 0);
                scalar /= (double)Model.Anomaly.Layers[k].Thickness;
                scalar *= normalizeFactor;

                _backwardFactors[k] = scalar;
            }
        }

        private void PrepareRFunction(Complex[] zeta0)
        {
            var nx = Model.Anomaly.LocalSize.Nx;
            var ny = Model.Anomaly.LocalSize.Ny;
            var nz = Model.Nz;

            var conjZBkg = new Complex[nz];
            var sqrtReZetaBkg = new Complex[nz];
            var zetas = Model.Anomaly.Zeta;

            for (int k = 0; k < nz; k++)
            {
                var zetaBkg = zeta0[k];
                conjZBkg[k] = Complex.Conjugate(zetaBkg);
                sqrtReZetaBkg[k] = Complex.Sqrt(zetaBkg.Real);
            }

            long index = 0;

            for (int i = 0; i < nx; i++)
                for (int j = 0; j < ny; j++)
                    for (int k = 0; k < nz; k++)
                    {
                        var zeta = zetas[i, j, k];

                        if (_operatorType == OperatorType.Chi0)
                            _rFunction[index++] = sqrtReZetaBkg[k] / (zeta + conjZBkg[k]);
                        if (_operatorType == OperatorType.A)
                            _rFunction[index++] = (zeta - zeta0[k]) / (zeta + conjZBkg[k]);
                    }
        }

		private void PrepareGiem2gOperator ()
		{
			var nx = Model.Anomaly.LocalSize.Nx;
			var ny = Model.Anomaly.LocalSize.Ny;
			var nz = Model.Nz;
			var zetas = Model.Anomaly.Zeta;

			long index = 0;
			for (int i = 0; i < nx; i++)
				for (int j = 0; j < ny; j++)
					for (int k = 0; k < nz; k++)
					{
					_rFunction [index++] = zetas [i, j, k];
					}
			Giem2gGreenTensor.PrepareAnomalyConductivity (_greenTensor, _rFunction);
		}

		private void CalculateOperatorAorKr()
        {
            DoWithProfiling(ApplyOperatorR, ProfilerEvent.OperatorAApplyR);
            DoWithProfiling(PrepareForForwardFft, ProfilerEvent.OperatorAPrepareForForwardFft);
            DoWithProfiling(PerformForwardFft, ProfilerEvent.OperatorAForwardFft);
            DoWithProfiling(ApplyGreenTensorAlongZ, ProfilerEvent.OperatorAMultiplication);
            DoWithProfiling(PerformBackwardFft, ProfilerEvent.OperatorABackwardFft);
            DoWithProfiling(ExtractData, ProfilerEvent.OperatorAExtractAfterBackwardFft);
            DoWithProfiling(PerformLastStep, ProfilerEvent.OperatorAFinish);
        }

		void ApplyGiem2gOperator ()
		{
			Giem2gGreenTensor.Apply (_greenTensor, _input.Ptr, _output.Ptr);

		}

        private void DoWithProfiling(Action action, ProfilerEvent profilerEvent)
        {
            using (Profiler?.StartAuto(profilerEvent))
            {
                action();
            }
        }

        private void PerformLastStep()
        {
            var length = 3L * _output.Nz * _output.Nx * _output.Ny;

            if (_operatorType == OperatorType.A)
            {
                UNM.SubtractElementwise(length, _input.Ptr, _output.Ptr, _output.Ptr);
                UNM.SubtractElementwise(length, _output.Ptr, _rx.Ptr, _output.Ptr);
            }

            if (_operatorType == OperatorType.Chi0)
            {
                UNM.AddElementwise(length, _output.Ptr, _rx.Ptr, _output.Ptr);
            }
        }

        private void PerformBackwardFft()
            => Pool.ExecuteBackward(Pool.Plan3Nz);

        private void PerformForwardFft()
            => Pool.ExecuteForward(Pool.Plan3Nz);

        private void ExtractData()
        {
            int nx = _output.Nx;
            int ny = _output.Ny;
            int nz = Model.Nz;

            var output = Pool.Plan3Nz.Buffer2Ptr;

            Iterate(nx, i => //for (int i = 0; i < nx; i++)
            {
                for (int j = 0; j < ny; j++)
                {
                    long shiftDst = (i * ny + j) * 3L * nz;
                    long shiftSrc = (i * ny * 2 + j) * 3L * nz;

                    UNM.MultiplyElementwise(nz, _backwardFactors, output + shiftSrc, _output.Ptr + shiftDst);
                    UNM.MultiplyElementwise(nz, _backwardFactors, output + shiftSrc + nz,
                        _output.Ptr + shiftDst + nz);
                    UNM.MultiplyElementwise(nz, _backwardFactors, output + shiftSrc + nz + nz,
                        _output.Ptr + shiftDst + nz + nz);
                }
            });
        }

        private void ApplyOperatorR()
        {
            int length = _rx.Nx * _rx.Ny;
            int nz = _rx.Nz;

            Iterate(length, i => //for (int i = 0; i < length; i++)
            {
                long inz = i * nz;
                long inz3 = i * nz * 3;

                UNM.MultiplyElementwise(nz, _rFunction + inz, _input.Ptr + inz3, _rx.Ptr + inz3);
                UNM.MultiplyElementwise(nz, _rFunction + inz, _input.Ptr + inz3 + nz, _rx.Ptr + inz3 + nz);
                UNM.MultiplyElementwise(nz, _rFunction + inz, _input.Ptr + inz3 + nz + nz, _rx.Ptr + inz3 + nz + nz);
                //}
            });
        }

        private void PrepareForForwardFft()
        {
            var inputPtr = Pool.Plan3Nz.Buffer1Ptr;
            UNM.ClearBuffer(inputPtr, Pool.Plan3Nz.BufferLength);

            int nx = _rx.Nx;
            int ny = _rx.Ny;
            int nz = Model.Nz;

            Iterate(nx, i => //for (int i = 0; i < nx; i++)
            {
                for (int j = 0; j < ny; j++)
                {
                    long shiftSrc = (i * ny + j) * 3L * nz;
                    long shiftDst = (i * ny * 2 + j) * 3L * nz;

                    UNM.MultiplyElementwise(nz, _forwardFactors, _rx.Ptr + shiftSrc, inputPtr + shiftDst);
                    UNM.MultiplyElementwise(nz, _forwardFactors, _rx.Ptr + shiftSrc + nz, inputPtr + shiftDst + nz);
                    UNM.MultiplyElementwise(nz, _forwardFactors, _rx.Ptr + shiftSrc + nz + nz,
                        inputPtr + shiftDst + nz + nz);
                }
            });
        }

        private void ApplyGreenTensorAlongZ()
        {
            var input = Pool.Plan3Nz.Buffer1Ptr;
            var result = Pool.Plan3Nz.Buffer2Ptr;

            int nz = Model.Nz;
            long asymNz = nz * nz;
            long symmNz = nz + nz * (nz - 1) / 2;
            int length = Pool.Plan3Nz.BufferLength / (3 * nz);

            var xx = _greenTensor["xx"].Ptr;
            var xy = _greenTensor["xy"].Ptr;
            var xz = _greenTensor["xz"].Ptr;


            var yy = _greenTensor["yy"].Ptr;
            var yz = _greenTensor["yz"].Ptr;

            var zz = _greenTensor["zz"].Ptr;

            Iterate(length, i =>
            {
                //for (int i = 0; i < length; i++)
                //  {
                long dataShift = i * 3L * nz;
                long symmShift = i * symmNz;
                long asymShift = i * asymNz;

                var dstX = result + dataShift;
                var dstY = result + dataShift + nz;
                var dstZ = result + dataShift + nz + nz;

                var srcX = input + dataShift;
                var srcY = input + dataShift + nz;
                var srcZ = input + dataShift + nz + nz;

                var one = Complex.One;
                var minusOne = -Complex.One;
                var zero = Complex.Zero;

                UNM.ZgemvSym(nz, &one, &zero, xx + symmShift, srcX, dstX);
                UNM.ZgemvSym(nz, &one, &one, xy + symmShift, srcY, dstX);
                UNM.ZgemvAsymNoTrans(nz, &one, &one, xz + asymShift, srcZ, dstX);

                UNM.ZgemvSym(nz, &one, &zero, xy + symmShift, srcX, dstY);
                UNM.ZgemvSym(nz, &one, &one, yy + symmShift, srcY, dstY);
                UNM.ZgemvAsymNoTrans(nz, &one, &one, yz + asymShift, srcZ, dstY);

                UNM.ZgemvAsymTrans(nz, &minusOne, &zero, xz + asymShift, srcX, dstZ);
                UNM.ZgemvAsymTrans(nz, &minusOne, &one, yz + asymShift, srcY, dstZ);
                UNM.ZgemvSym(nz, &one, &one, zz + symmShift, srcZ, dstZ);
                //     }
            });
        }

        private void Iterate(int length, Action<int> action)
        {
            var options = MultiThreadUtils.CreateParallelOptions();
            System.Threading.Tasks.Parallel.For(0, length, options, action);
        }

        private Complex* Alloc(long numberOfElements)
         => _solver.MemoryProvider.AllocateComplex(numberOfElements);

        public void Dispose()
        {
            ReleaseMemory(_rFunction);
            ReleaseMemory(_backwardFactors);
            ReleaseMemory(_forwardFactors);
            _rx.Dispose();
        }

        private void ReleaseMemory(Complex* ptr)
            => _solver.MemoryProvider.Release(ptr);
    }
}
