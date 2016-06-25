//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
﻿using System;
using System.Numerics;
using Extreme.Cartesian.Model;
using UNM = Extreme.Cartesian.Forward.UnsafeNativeMethods;

namespace Extreme.Cartesian.Green.Tensor.Impl
{
    public unsafe class GreenTensorFftPopulator
    {
        private readonly GreenTensor _greenTensor;

        private readonly long _nx;
        private readonly long _ny;
        private readonly long _nx2;
        private readonly long _ny2;
        private readonly long _length;

        private int _layoutStep;
        private long _layoutShiftFactor;


        public GreenTensorFftPopulator(GreenTensor greenTensor, OmegaModel model, bool symm = false)
        {
            _greenTensor = greenTensor;

            _nx = model.LateralDimensions.Nx;
            _ny = model.LateralDimensions.Ny;
            _nx2 = _nx * 2;
            _ny2 = _ny * 2;

            var nz = model.Anomaly.Layers.Count;

            _length = symm ? nz + nz * (nz - 1) / 2 : nz * nz;
        }

        public void PopulateForFft(MemoryLayoutOrder layoutOrder)
        {
            PrepareLayoutOrder(layoutOrder);

            if (_greenTensor.Has("XX")) PopulateForFft(_greenTensor["XX"], 1, 1);
            if (_greenTensor.Has("YY")) PopulateForFft(_greenTensor["YY"], 1, 1);
            if (_greenTensor.Has("ZZ")) PopulateForFft(_greenTensor["ZZ"], 1, 1);

            if (_greenTensor.Has("XY")) PopulateForFft(_greenTensor["XY"], -1, -1);

            if (_greenTensor.Has("XZ")) PopulateForFft(_greenTensor["XZ"], -1, 1);
            if (_greenTensor.Has("YZ")) PopulateForFft(_greenTensor["YZ"], 1, -1);

            if (_greenTensor.Has("ZX")) PopulateForFft(_greenTensor["ZX"], -1, 1);
            if (_greenTensor.Has("ZY")) PopulateForFft(_greenTensor["ZY"], 1, -1);
        }

        public void PopulateForFftDistributedAlongX(MemoryLayoutOrder layoutOrder, int nxLength)
        {
            PrepareLayoutOrder(layoutOrder);

            if (_greenTensor.Has("XX")) PopulateForFftDistributedAlongX(nxLength, _greenTensor["XX"], 1);
            if (_greenTensor.Has("YY")) PopulateForFftDistributedAlongX(nxLength, _greenTensor["YY"], 1);
            if (_greenTensor.Has("ZZ")) PopulateForFftDistributedAlongX(nxLength, _greenTensor["ZZ"], 1);

            if (_greenTensor.Has("XY")) PopulateForFftDistributedAlongX(nxLength, _greenTensor["XY"], -1);

            if (_greenTensor.Has("XZ")) PopulateForFftDistributedAlongX(nxLength, _greenTensor["XZ"], 1);
            if (_greenTensor.Has("YZ")) PopulateForFftDistributedAlongX(nxLength, _greenTensor["YZ"], -1);

            if (_greenTensor.Has("ZX")) PopulateForFftDistributedAlongX(nxLength, _greenTensor["ZX"], 1);
            if (_greenTensor.Has("ZY")) PopulateForFftDistributedAlongX(nxLength, _greenTensor["ZY"], -1);
        }


        private void PrepareLayoutOrder(MemoryLayoutOrder layoutOrder)
        {
            if (layoutOrder == MemoryLayoutOrder.AlongLateral)
            {
                _layoutStep = (int)(_nx * _ny * 4);
                _layoutShiftFactor = 1;
            }
            else if (layoutOrder == MemoryLayoutOrder.AlongVertical)
            {
                _layoutStep = 1;
                _layoutShiftFactor = _length;
            }
            else
                throw new InvalidOperationException(nameof(layoutOrder));
        }

        private void PopulateForFft(GreenTensor.Component q, int xSym, int ySym)
        {
            for (int i = 1; i < _nx; i++)
                for (int j = 1; j < _ny; j++)
                    PopulateGreen(q.Ptr, xSym, ySym, i, j);

            for (int i = 1; i < _nx; i++)
                PopulateGreenAlongX(q.Ptr, xSym, i, _nx2 - i);

            for (int j = 1; j < _ny; j++)
                PopulateGreenAlongY(q.Ptr, ySym, j, _ny2 - j);
        }

        private void PopulateForFftDistributedAlongX(int nxLength, GreenTensor.Component q, int ySym)
        {
            for (int i = 0; i < nxLength; i++)
            {
                for (int j = 1; j < _ny; j++)
                    PopulateGreenAlongY(q.Ptr, ySym, i, j, _ny2 - j);
            }
        }

        private void PopulateGreen(Complex* ptr, int xSym, int ySym, int i, int j)
        {
            long shift = _layoutShiftFactor * (i * _ny2 + j);

            long shiftXSym = _layoutShiftFactor * ((_nx2 - i) * _ny2 + j);
            long shiftYSym = _layoutShiftFactor * (i * _ny2 + _ny2 - j);
            long shiftXYSym = _layoutShiftFactor * ((_nx2 - i) * _ny2 + _ny2 - j);

            var src = ptr + shift;

            UNM.Zaxpy(_length, xSym, src, _layoutStep, ptr + shiftXSym, _layoutStep);
            UNM.Zaxpy(_length, ySym, src, _layoutStep, ptr + shiftYSym, _layoutStep);
            UNM.Zaxpy(_length, xSym * ySym, src, _layoutStep, ptr + shiftXYSym, _layoutStep);
        }

        private void PopulateGreenAlongX(Complex* ptr, int xSym, long srcI, long dstI)
        {
            long shift = _layoutShiftFactor * (srcI * _ny2);
            long shiftXSym = _layoutShiftFactor * (dstI * _ny2);

            UNM.Zaxpy(_length, xSym, ptr + shift, _layoutStep, ptr + shiftXSym, _layoutStep);
        }


        private void PopulateGreenAlongY(Complex* ptr, int ySym, int i, long srcJ, long dstJ)
        {
            long shift = _layoutShiftFactor * (i * _ny2 + srcJ);
            long shiftYSym = _layoutShiftFactor * (i * _ny2 + dstJ);

            UNM.Zaxpy(_length, ySym, ptr + shift, _layoutStep, ptr + shiftYSym, _layoutStep);
        }

        private void PopulateGreenAlongY(Complex* ptr, int ySym, long srcJ, long dstJ)
        {
            long shift = _layoutShiftFactor * srcJ;
            long shiftYSym = _layoutShiftFactor * dstJ;

            UNM.Zaxpy(_length, ySym, ptr + shift, _layoutStep, ptr + shiftYSym, _layoutStep);
        }
    }
}
