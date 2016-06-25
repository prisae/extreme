//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
﻿using System;
using System.Numerics;
using Extreme.Cartesian.Core;
using Extreme.Cartesian.Green.Tensor;
using UNF = Extreme.Cartesian.Forward.UnsafeNativeMethods;

namespace Extreme.Cartesian.Forward
{
    public abstract unsafe class ToOCalculator : ForwardSolverComponent
    {
        protected ToOCalculator(ForwardSolver solver) : base(solver)
        {
        }

        #region Level
        protected void CalculateAnomalyFieldE(AnomalyCurrent src, AnomalyCurrent dst, GreenTensor greenTensor)
            => CalculateAnomalyField(src, dst, greenTensor, electric: true);
        protected void CalculateAnomalyFieldH(AnomalyCurrent src, AnomalyCurrent dst, GreenTensor greenTensor)
            => CalculateAnomalyField(src, dst, greenTensor, electric: false);

        private void CalculateAnomalyField(AnomalyCurrent src, AnomalyCurrent dst, GreenTensor gt, bool electric)
        {
            var forward = src.Nz == 1 ? Pool.Plan3 : Pool.Plan3Nz;
            var backward = dst.Nz == 1 ? Pool.Plan3 : Pool.Plan3Nz;


            UNF.ClearBuffer(forward.Buffer1Ptr, forward.BufferLength);
            PrepareForForwardFft(src, forward.Buffer1Ptr);
            Pool.ExecuteForward(forward);


            //Clear(backward.Input);
            if (electric)
                ApplyGreenTensorAlongZForE(gt, forward.Buffer1Ptr, backward.Buffer2Ptr);
            else
                ApplyGreenTensorAlongZForH(gt, forward.Buffer1Ptr, backward.Buffer2Ptr);

            Pool.ExecuteBackward(backward);

            var scalar = new Complex(1 / ((double)Model.Nx * (double)Model.Ny * 4), 0);
            Zscal(3 * dst.Nx * 2 * dst.Ny * 2 * dst.Nz, scalar, backward.Buffer2Ptr);

            ExtractData(backward.Buffer2Ptr, dst);
        }

        #endregion

        private static void ApplyGreenTensorAlongZForE(GreenTensor gt, Complex* input, Complex* result)
        {
            int gtNz = gt.NTr * gt.NRc;
            int srcNz = gt.NTr;
            int dstNz = gt.NRc;
            int length = gt.Nx * gt.Ny;

            //int srcNz = input.Nz;
            //int dstNz = result.Nz;
            //int length = input.Nx * input.Ny;

            //for (int i = 0; i < length; i++)
            Iterate(length, i =>
            {
                int dstShift = i * 3 * dstNz;
                int srcShift = i * 3 * srcNz;
                int gtShift = i * gtNz;

                var dstX = result + dstShift;
                var dstY = result + dstShift + dstNz;
                var dstZ = result + dstShift + dstNz + dstNz;

                var srcX = input + srcShift;
                var srcY = input + srcShift + srcNz;
                var srcZ = input + srcShift + srcNz + srcNz;

                Zgemv(srcNz, dstNz, Complex.One, Complex.Zero, gt["xx"][gtShift], srcX, dstX);
                Zgemv(srcNz, dstNz, Complex.One, Complex.One, gt["xy"][gtShift], srcY, dstX);
                Zgemv(srcNz, dstNz, Complex.One, Complex.One, gt["xz"][gtShift], srcZ, dstX);

                Zgemv(srcNz, dstNz, Complex.One, Complex.Zero, gt["xy"][gtShift], srcX, dstY);
                Zgemv(srcNz, dstNz, Complex.One, Complex.One, gt["yy"][gtShift], srcY, dstY);
                Zgemv(srcNz, dstNz, Complex.One, Complex.One, gt["yz"][gtShift], srcZ, dstY);

                Zgemv(srcNz, dstNz, Complex.One, Complex.Zero, gt["zx"][gtShift], srcX, dstZ);
                Zgemv(srcNz, dstNz, Complex.One, Complex.One, gt["zy"][gtShift], srcY, dstZ);
                Zgemv(srcNz, dstNz, Complex.One, Complex.One, gt["zz"][gtShift], srcZ, dstZ);
                //}
            });
        }

        private static void ApplyGreenTensorAlongZForH(GreenTensor gt, Complex* input, Complex* result)
        {
            int gtNz = gt.NTr * gt.NRc;
            int srcNz = gt.NTr;
            int dstNz = gt.NRc;

            int length = gt.Nx * gt.Ny;

            Iterate(length, i =>
            //for (int i = 0; i < length; i++)
            {
                int dstShift = i * 3 * dstNz;
                int srcShift = i * 3 * srcNz;
                int gtShift = i * gtNz;

                var dstX = result + dstShift;
                var dstY = result + dstShift + dstNz;
                var dstZ = result + dstShift + dstNz + dstNz;

                var srcX = input + srcShift;
                var srcY = input + srcShift + srcNz;
                var srcZ = input + srcShift + srcNz + srcNz;

                Zgemv(srcNz, dstNz, -Complex.One, Complex.Zero, gt["xx"][gtShift], srcX, dstX);
                Zgemv(srcNz, dstNz, -Complex.One, Complex.One, gt["xy"][gtShift], srcY, dstX);
                Zgemv(srcNz, dstNz, -Complex.One, Complex.One, gt["xz"][gtShift], srcZ, dstX);

                Zgemv(srcNz, dstNz, Complex.One, Complex.Zero, gt["yx"][gtShift], srcX, dstY);
                Zgemv(srcNz, dstNz, Complex.One, Complex.One, gt["xx"][gtShift], srcY, dstY);
                Zgemv(srcNz, dstNz, Complex.One, Complex.One, gt["yz"][gtShift], srcZ, dstY);

                Zgemv(srcNz, dstNz, -Complex.One, Complex.Zero, gt["zx"][gtShift], srcX, dstZ);
                Zgemv(srcNz, dstNz, Complex.One, Complex.One, gt["zy"][gtShift], srcY, dstZ);
                //}
            });
        }


        private static void Iterate(int length, Action<int> action)
        {
            var options = MultiThreadUtils.CreateParallelOptions();
            System.Threading.Tasks.Parallel.For(0, length, options, action);
        }

        private static void PrepareForForwardFft(AnomalyCurrent src, Complex* inputPtr)
        {
            int nx = src.Nx;
            int ny = src.Ny;
            int nz = src.Nz;

            for (int i = 0; i < nx; i++)
                for (int j = 0; j < ny; j++)
                {
                    long shiftSrc = (i * ny + j) * 3 * nz;
                    long shiftDst = (i * ny * 2 + j) * 3 * nz;

                    Copy(3 * nz, src.Ptr + shiftSrc, inputPtr + shiftDst);
                }
        }

        private static void ExtractData(Complex* outputFftPtr, AnomalyCurrent field)
        {
            int nx = field.Nx;
            int ny = field.Ny;
            int nz = field.Nz;

            for (int i = 0; i < nx; i++)
                for (int j = 0; j < ny; j++)
                {
                    long shiftDst = (i * ny + j) * 3 * nz;
                    long shiftSrc = (i * ny * 2 + j) * 3 * nz;

                    //Copy(3 * nz, outputFft.Ptr + shiftSrc, field.Ptr + shiftDst);
                    Zaxpy(3 * nz, Complex.One, outputFftPtr + shiftSrc, field.Ptr + shiftDst);
                }
        }

        private static void Zgemv(int n, int m, Complex a, Complex b, Complex* gt, Complex* src, Complex* dst)
        {
            UNF.ZgemvAtoO(n, m, &a, &b, gt, src, dst);
        }

        private static void Zaxpy(long n, Complex alpha, Complex* x, Complex* y)
            => UNF.Zaxpy(n, alpha, x, y);

        protected static void Copy(long length, Complex* ptr, Complex* complexPtr)
            => UNF.Zcopy(length, ptr, complexPtr);

        private void Zscal(long length, Complex alpha, Complex* ptr)
            => UNF.Zscal(length, alpha, ptr);
    }
}
