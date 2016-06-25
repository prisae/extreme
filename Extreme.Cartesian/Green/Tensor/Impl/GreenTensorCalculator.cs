//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
﻿using System;
using System.Linq;
using System.Numerics;

using Extreme.Core;
using Extreme.Cartesian.Core;
using Extreme.Cartesian.Green.Scalar;
using Extreme.Cartesian.Logger;
using Extreme.Cartesian.Model;

using UNM = Extreme.Cartesian.Forward.UnsafeNativeMethods;

namespace Extreme.Cartesian.Green.Tensor.Impl
{
    public abstract unsafe class GreenTensorCalculator
    {
        public GreenTensorKnot[,] _knots;
        public GreenTensorKnot[] _zeroKnotsX;
        public GreenTensorKnot[] _zeroKnotsY;

        private QBuffer _qBuffer;

        private ScalarSegments _segments;
        private GreenTensor _greenTensor;
        private double[] _radii;

        private int _layoutShiftFactor;
        private int _layoutStep;

        public bool KnotsAreReady { get; private set; }

        protected ILogger Logger { get; }
        protected INativeMemoryProvider MemoryProvider { get; }
        protected OmegaModel Model { get; }


        protected bool CalculateXx;
        protected bool CalculateXy;
        protected bool CalculateXz;

        protected bool CalculateYx;
        protected bool CalculateYy;
        protected bool CalculateYz;

        protected bool CalculateZx;
        protected bool CalculateZy;
        protected bool CalculateZz;

        protected int Nx => Model.LateralDimensions.Nx;
        protected int Ny => Model.LateralDimensions.Ny;
        protected int Nz => Model.Anomaly.Layers.Count;

        protected GreenTensorCalculator(
            ILogger logger,
            OmegaModel model,
            INativeMemoryProvider memoryProvider)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            if (memoryProvider == null) throw new ArgumentNullException(nameof(memoryProvider));
            if (model == null) throw new ArgumentNullException(nameof(model));

            Logger = logger;
            MemoryProvider = memoryProvider;
            Model = model;
        }

        protected void SetGreenTensorAndRadii(GreenTensor greenTensor, double[] radii)
        {
            if (greenTensor == null) throw new ArgumentNullException(nameof(greenTensor));
            if (radii == null) throw new ArgumentNullException(nameof(radii));

            _greenTensor = greenTensor;
            _radii = radii;
        }

        private static void Iterate(int fromInclusive, int toExclusive, Action<int> action)
        {
            var options = MultiThreadUtils.CreateParallelOptions();

            System.Threading.Tasks.Parallel.For(fromInclusive, toExclusive, options, action);
        }

        #region Prepare Layout And Values

        protected void SetQBufferSize(int size)
        {
            _qBuffer = new QBuffer();

            if (CalculateXx) _qBuffer.Qxx = new Complex[size];
            if (CalculateXy) _qBuffer.Qxy = new Complex[size];
            if (CalculateXz) _qBuffer.Qxz = new Complex[size];

            if (CalculateYx) _qBuffer.Qyx = new Complex[size];
            if (CalculateYy) _qBuffer.Qyy = new Complex[size];
            if (CalculateYz) _qBuffer.Qyz = new Complex[size];

            if (CalculateZx) _qBuffer.Qzx = new Complex[size];
            if (CalculateZy) _qBuffer.Qzy = new Complex[size];
            if (CalculateZz) _qBuffer.Qzz = new Complex[size];
        }

        protected void SetCalculateAll(bool value)
        {
            CalculateXx = value;
            CalculateXy = value;
            CalculateXz = value;

            CalculateYx = value;
            CalculateYy = value;
            CalculateYz = value;

            CalculateZx = value;
            CalculateZy = value;
            CalculateZz = value;
        }

        protected void PrepareLayoutOrder(MemoryLayoutOrder layoutOrder, int nx, int ny, int nTr, int nRc)
        {
            if (layoutOrder == MemoryLayoutOrder.AlongLateral)
            {
                _layoutStep = nx * ny;
                _layoutShiftFactor = 1;
            }
            else if (layoutOrder == MemoryLayoutOrder.AlongVertical)
            {
                _layoutStep = 1;
                _layoutShiftFactor = nTr * nRc;

            }
            else
                throw new InvalidOperationException(nameof(layoutOrder));
        }

        protected void PrepareLayoutOrderSymm(MemoryLayoutOrder layoutOrder, int nx, int ny, int nz)
        {
            if (layoutOrder == MemoryLayoutOrder.AlongLateral)
            {
                _layoutStep = nx * ny;
                _layoutShiftFactor = 1;
            }
            else if (layoutOrder == MemoryLayoutOrder.AlongVertical)
            {
                _layoutStep = 1;
                _layoutShiftFactor = nz + nz * (nz - 1) / 2;

            }
            else
                throw new InvalidOperationException(nameof(layoutOrder));
        }

        protected void SetSegments(ScalarSegments segments)
        {
            _segments = segments;
        }

        #endregion

        #region Knots Preparation

        public void ResetKnots()
        {
            KnotsAreReady = false;
        }

        protected void PrepareKnots(int leftX, int rightX, double xShift,
                                    int leftY, int rightY, double yShift,
                                    double[] radii)
        {
            if (KnotsAreReady)
                throw new InvalidOperationException("knots are already ready");

            var dx = (double)Model.LateralDimensions.CellSizeX;
            var dy = (double)Model.LateralDimensions.CellSizeY;

            _knots = new GreenTensorKnot[rightX - leftX + 1, rightY - leftY + 1];
            _zeroKnotsX = new GreenTensorKnot[rightY - leftY + 1];
            _zeroKnotsY = new GreenTensorKnot[rightX - leftX + 1];

            for (int i = leftX; i <= rightX; i++)
            {
                var x = dx * (i - 0.5) - xShift;

                for (int j = leftY; j <= rightY; j++)
                {
                    var y = dy * (j - 0.5) - yShift;
                    _knots[i - leftX, j - leftY] = PrepareKnot(radii, x, y);
                }

            }

            for (int j = leftY; j <= rightY; j++)
            {
                var y = dy * (j - 0.5) - yShift;
                _zeroKnotsX[j - leftY] = PrepareKnot(radii, 0, y);
            }

            for (int i = leftX; i <= rightX; i++)
            {
                var x = dx * (i - 0.5) - xShift;
                _zeroKnotsY[i - leftX] = PrepareKnot(radii, x, 0);
            }

            KnotsAreReady = true;
        }

        private GreenTensorKnot PrepareKnot(double[] radii, double x, double y)
        {
            var r = Math.Sqrt(x * x + y * y);
            var index = Array.BinarySearch(radii, r);

            if (index < 0)
                index = ~index - 1;

            if (index == radii.Length - 1)
            {
                Logger.WriteError($"max radius {radii.Last()} is not enough, need {r}, wrong scalar plan");
                index--;
            }

            if (index < 0)
            {
                Logger.WriteError($"MIN radius { radii[0]} is not enough, need {r}, wrong scalar plan");
                index = 0;
            }

            return new GreenTensorKnot(index, r);
        }


        #endregion

        #region Run Along ...

        protected void RunAlongXElectric() =>
               RunAlongXElectric(leftX: 0, rightX: Nx, xShift: 0,
                                 leftY: 0, rightY: Ny, yShift: 0);



        protected void RunAlongXElectric(int leftX, int rightX, double xShift,
                                         int leftY, int rightY, double yShift)
            => RunAlongX(leftX: leftX, rightX: rightX, xShift: xShift,
                         leftY: leftY, rightY: rightY, yShift: yShift,
                         addToBuf: AddToQBuffAlongXElectric, qSigns: QSigns.AlongX, qSignsSymm: QSigns.AlongXSymm);


        protected void RunAlongXMagnetic(int leftX, int rightX, double xShift,
                                         int leftY, int rightY, double yShift)
            => RunAlongX(leftX: leftX, rightX: rightX, xShift: xShift,
                         leftY: leftY, rightY: rightY, yShift: yShift,
                         addToBuf: AddToQBuffAlongXMagnetic, qSigns: QSigns.AlongXMagnetic, qSignsSymm: QSigns.AlongXSymmMagnetic);

        private void RunAlongX(int leftX, int rightX, double xShift,
                               int leftY, int rightY, double yShift,
                               Action<IntegralFactors, int> addToBuf, QSigns qSigns, QSigns qSignsSymm)
        {
            var dx = (double)Model.LateralDimensions.CellSizeX;
            var dy = (double)Model.LateralDimensions.CellSizeY;

            //Iterate(leftY, rightY, j =>

            for (int j = leftY; j <= rightY; j++)
            {
                var y = dy * (j - 0.5) - yShift;
                var jIndex = j - leftY;

                for (int i = leftX; i <= rightX - 1; i++)
                {
                    var x1 = dx * (i - 0.5) - xShift;
                    var x2 = dx * (i + 0.5) - xShift;
                    var iIndex = i - leftX;

                    if (x1 > 0 && x2 > 0)
                    {
                        Integrate(addToBuf, _knots[iIndex, jIndex], _knots[iIndex + 1, jIndex], x1, x2, y);
                        FillResultAlongX(i, j, leftY, rightY, qSigns);
                    }

                    else if (x1 < 0 && x2 < 0)
                    {
                        Integrate(addToBuf, _knots[iIndex + 1, jIndex], _knots[iIndex, jIndex], -x2, -x1, y);
                        FillResultAlongX(i, j, leftY, rightY, qSignsSymm);
                    }

                    else
                    {
                        Integrate(addToBuf, _zeroKnotsX[jIndex], _knots[iIndex, jIndex], 0, -x1, y);
                        FillResultAlongX(i, j, leftY, rightY, qSignsSymm);

                        Integrate(addToBuf, _zeroKnotsX[jIndex], _knots[iIndex + 1, jIndex], 0, x2, y);
                        FillResultAlongX(i, j, leftY, rightY, qSigns);
                    }

                }
            }
            //});
        }



        protected void RunAlongYElectric() =>
                RunAlongYElectric(leftX: 0, rightX: Nx, xShift: 0,
                                  leftY: 0, rightY: Ny, yShift: 0);

        protected void RunAlongYElectric(int leftX, int rightX, double xShift,
                                       int leftY, int rightY, double yShift)
         => RunAlongY(leftX: leftX, rightX: rightX, xShift: xShift,
                      leftY: leftY, rightY: rightY, yShift: yShift,
                      addToBuf: AddToQBuffAlongYElectric, qSigns: QSigns.AlongY, qSignsSymm: QSigns.AlongYSymm);

        protected void RunAlongYMagnetic(int leftX, int rightX, double xShift,
                                       int leftY, int rightY, double yShift)
        => RunAlongY(leftX: leftX, rightX: rightX, xShift: xShift,
                     leftY: leftY, rightY: rightY, yShift: yShift,
                     addToBuf: AddToQBuffAlongYMagnetic, qSigns: QSigns.AlongYMagnetic, qSignsSymm: QSigns.AlongYSymmMagnetic);

        private void RunAlongY(int leftX, int rightX, double xShift,
                               int leftY, int rightY, double yShift,
                               Action<IntegralFactors, int> addToBuf, QSigns qSigns, QSigns qSignsSymm)
        {
            var dx = (double)Model.LateralDimensions.CellSizeX;
            var dy = (double)Model.LateralDimensions.CellSizeY;

            for (int i = leftX; i <= rightX; i++)
            {
                var x = dx * (i - 0.5) - xShift;
                var iIndex = i - leftX;


                //Iterate(leftY, rightY, j =>
                for (int j = leftY; j <= rightY - 1; j++)
                {
                    // Console.WriteLine($"{Thread.CurrentThread.ManagedThreadId} {j}");

                    var y1 = dy * (j - 0.5) - yShift;
                    var y2 = dy * (j + 0.5) - yShift;
                    var jIndex = j - leftY;

                    if (y1 > 0 && y2 > 0)
                    {
                        Integrate(addToBuf, _knots[iIndex, jIndex], _knots[iIndex, jIndex + 1], y1, y2, x);
                        FillResultAlongY(i, j, leftX, rightX, qSigns);
                    }
                    else if (y1 < 0 && y2 < 0)
                    {
                        Integrate(addToBuf, _knots[iIndex, jIndex + 1], _knots[iIndex, jIndex], -y2, -y1, x);
                        FillResultAlongY(i, j, leftX, rightX, qSignsSymm);
                    }
                    else
                    {
                        Integrate(addToBuf, _zeroKnotsY[iIndex], _knots[iIndex, jIndex], 0, -y1, x);
                        FillResultAlongY(i, j, leftX, rightX, qSignsSymm);

                        Integrate(addToBuf, _zeroKnotsY[iIndex], _knots[iIndex, jIndex + 1], 0, y2, x);
                        FillResultAlongY(i, j, leftX, rightX, qSigns);
                    }
                }
                //});
            }
        }

        #endregion

        #region Integrate

        private void Integrate(Action<IntegralFactors, int> addIntegralSegmentToQ, GreenTensorKnot left, GreenTensorKnot right, double s, double u, double v)
        {
            ClearQBuffer();

            int i1 = left.Index;
            int i2 = right.Index;

            if (i1 == i2)
            {
                var factors = IntegralFactors.Prepare(s, u, left.Radius, right.Radius, v);
                addIntegralSegmentToQ(factors, i1);
            }

            else
            {
                var r1 = left.Radius;
                var r2 = _radii[i1 + 1];

                var sj = s;
                var uj = Math.Sqrt(r2 * r2 - v * v);

                var factors = IntegralFactors.Prepare(sj, uj, r1, r2, v);
                addIntegralSegmentToQ(factors, i1);

                for (int i = i1 + 1; i < i2; i++)
                {
                    r1 = r2;
                    r2 = _radii[i + 1];

                    sj = uj;
                    uj = Math.Sqrt(r2 * r2 - v * v);

                    factors = IntegralFactors.Prepare(sj, uj, r1, r2, v);
                    addIntegralSegmentToQ(factors, i);
                }

                sj = uj;
                uj = u;
                r1 = r2;
                r2 = right.Radius;

                factors = IntegralFactors.Prepare(sj, uj, r1, r2, v);
                addIntegralSegmentToQ(factors, i2);
            }
        }

        private void FillResultAlongX(int i, int j, int left, int right, QSigns qSigns)
        {
            if (j == left)
                FillGreen(i, left, -1, qSigns);
            else if (j == right)
                FillGreen(i, right - 1, 1, qSigns);
            else
            {
                FillGreen(i, j - 1, 1, qSigns);
                FillGreen(i, j, -1, qSigns);
            }
        }

        private void FillResultAlongY(int i, int j, int left, int right, QSigns qSigns)
        {
            if (i == left)
                FillGreen(left, j, -1, qSigns);
            else if (i == right)
                FillGreen(right - 1, j, 1, qSigns);
            else
            {
                FillGreen(i - 1, j, 1, qSigns);
                FillGreen(i, j, -1, qSigns);
            }
        }

        private void ClearQBuffer()
        {
            Action<Complex[]> clear =
                c => Array.Clear(c, 0, c.Length);

            clear(_qBuffer.Qxx);
            clear(_qBuffer.Qyy);
            clear(_qBuffer.Qzz);
            clear(_qBuffer.Qxy);
            clear(_qBuffer.Qyx);
            clear(_qBuffer.Qxz);
            clear(_qBuffer.Qyz);
            clear(_qBuffer.Qzx);
            clear(_qBuffer.Qzy);
        }

        protected virtual int TransformI(int i) => i;
        protected virtual int TransformJ(int j) => j;

        private readonly object _sync = new object();

        private void FillGreen(int i, int j, int sign, QSigns qSigns)
        {
            i = TransformI(i);
            j = TransformJ(j);

            int lineLength = _greenTensor.Ny;
            long shift = (i * lineLength + j) * _layoutShiftFactor;


            {
                if (CalculateXx)
                    FillGreen(_qBuffer.Qxx, _greenTensor["xx"], sign * qSigns.XX, shift, _layoutStep);
                if (CalculateXy)
                    FillGreen(_qBuffer.Qxy, _greenTensor["xy"], sign * qSigns.XY, shift, _layoutStep);
                if (CalculateXz)
                    FillGreen(_qBuffer.Qxz, _greenTensor["xz"], sign * qSigns.XZ, shift, _layoutStep);

                if (CalculateYx)
                    FillGreen(_qBuffer.Qyx, _greenTensor["yx"], sign * qSigns.YX, shift, _layoutStep);
                if (CalculateYy)
                    FillGreen(_qBuffer.Qyy, _greenTensor["yy"], sign * qSigns.YY, shift, _layoutStep);
                if (CalculateYz)
                    FillGreen(_qBuffer.Qyz, _greenTensor["yz"], sign * qSigns.YZ, shift, _layoutStep);

                if (CalculateZx)
                    FillGreen(_qBuffer.Qzx, _greenTensor["zx"], sign * qSigns.ZX, shift, _layoutStep);
                if (CalculateZy)
                    FillGreen(_qBuffer.Qzy, _greenTensor["zy"], sign * qSigns.ZY, shift, _layoutStep);
                if (CalculateZz)
                    FillGreen(_qBuffer.Qzz, _greenTensor["zz"], sign * qSigns.ZZ, shift, _layoutStep);
            }
        }

        private void FillGreen(Complex[] buff, GreenTensor.Component ca, int sign, long shift, int step)
        {
            fixed (Complex* q = &buff[0])
                UNM.Zaxpy(buff.Length, sign, q, ca.Ptr + shift, step);
        }

        private void FillQ(Complex* src, Complex[] dst, Complex alpha)
        {
            fixed (Complex* dstPtr = &dst[0])
                UNM.Zaxpy(dst.Length, alpha, src, dstPtr);
        }

        private void AddToQBuffAlongXElectric(IntegralFactors factors, int index)
        {
            var buffer = _qBuffer;
            var seg = _segments.SingleSegment[index];

            if (CalculateXx) { FillQ(seg.I1A, buffer.Qxx, factors.D2A); FillQ(seg.I1B, buffer.Qxx, factors.D2B); }
            if (CalculateYy) { FillQ(seg.I2A, buffer.Qyy, factors.D2A); FillQ(seg.I2B, buffer.Qyy, factors.D2B); }
            if (CalculateZz) { FillQ(seg.I5A, buffer.Qzz, factors.D2A); FillQ(seg.I5B, buffer.Qzz, factors.D2B); }
            if (CalculateXy) { FillQ(seg.I2A, buffer.Qxy, factors.D3A); FillQ(seg.I2B, buffer.Qxy, factors.D3B); }
            if (CalculateYz) { FillQ(seg.I3A, buffer.Qyz, factors.D1A); FillQ(seg.I3B, buffer.Qyz, factors.D1B); }
            if (CalculateZy) { FillQ(seg.I4A, buffer.Qzy, factors.D1A); FillQ(seg.I4B, buffer.Qzy, factors.D1B); }
        }
        private void AddToQBuffAlongYElectric(IntegralFactors factors, int index)
        {
            var buffer = _qBuffer;
            var seg = _segments.SingleSegment[index];

            if (CalculateXx) { FillQ(seg.I2A, buffer.Qxx, factors.D2A); FillQ(seg.I2B, buffer.Qxx, factors.D2B); }
            if (CalculateYy) { FillQ(seg.I1A, buffer.Qyy, factors.D2A); FillQ(seg.I1B, buffer.Qyy, factors.D2B); }
            if (CalculateZz) { FillQ(seg.I5A, buffer.Qzz, factors.D2A); FillQ(seg.I5B, buffer.Qzz, factors.D2B); }
            if (CalculateXy) { FillQ(seg.I1A, buffer.Qxy, factors.D3A); FillQ(seg.I1B, buffer.Qxy, factors.D3B); }
            if (CalculateXz) { FillQ(seg.I3A, buffer.Qxz, factors.D1A); FillQ(seg.I3B, buffer.Qxz, factors.D1B); }
            if (CalculateZx) { FillQ(seg.I4A, buffer.Qzx, factors.D1A); FillQ(seg.I4B, buffer.Qzx, factors.D1B); }
        }

        private void AddToQBuffAlongXMagnetic(IntegralFactors factors, int index)
        {
            var buffer = _qBuffer;
            var seg = _segments.SingleSegment[index];

            if (CalculateXx) { FillQ(seg.I2A, buffer.Qxx, factors.D3A); FillQ(seg.I2B, buffer.Qxx, factors.D3B); }
            if (CalculateYx) { FillQ(seg.I1A, buffer.Qyx, factors.D2A); FillQ(seg.I1B, buffer.Qyx, factors.D2B); }
            if (CalculateXy) { FillQ(seg.I2A, buffer.Qxy, factors.D2A); FillQ(seg.I2B, buffer.Qxy, factors.D2B); }
            if (CalculateXz) { FillQ(seg.I3A, buffer.Qxz, factors.D1A); FillQ(seg.I3B, buffer.Qxz, factors.D1B); }
            if (CalculateZx) { FillQ(seg.I4A, buffer.Qzx, factors.D1A); FillQ(seg.I4B, buffer.Qzx, factors.D1B); }
        }

        private void AddToQBuffAlongYMagnetic(IntegralFactors factors, int index)
        {
            var buffer = _qBuffer;
            var seg = _segments.SingleSegment[index];

            if (CalculateYx) { FillQ(seg.I2A, buffer.Qyx, factors.D2A); FillQ(seg.I2B, buffer.Qyx, factors.D2B); }
            if (CalculateXy) { FillQ(seg.I1A, buffer.Qxy, factors.D2A); FillQ(seg.I1B, buffer.Qxy, factors.D2B); }
            if (CalculateXx) { FillQ(seg.I1A, buffer.Qxx, factors.D3A); FillQ(seg.I1B, buffer.Qxx, factors.D3B); }
            if (CalculateYz) { FillQ(seg.I3A, buffer.Qyz, factors.D1A); FillQ(seg.I3B, buffer.Qyz, factors.D1B); }
            if (CalculateZy) { FillQ(seg.I4A, buffer.Qzy, factors.D1A); FillQ(seg.I4B, buffer.Qzy, factors.D1B); }
        }
        #endregion
    }
}
