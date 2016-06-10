using System;
using System.Linq;
using FftWrap;
using Extreme.Cartesian.FftW;
using Extreme.Cartesian.Green.Scalar;
using Extreme.Cartesian.Model;
using Extreme.Core;
using Extreme.Cartesian.Green.Tensor;

namespace Extreme.Cartesian.Green.Tensor.Impl
{
    public class AtoAGreenTensorCalculator : GreenTensorCalculator
    {
        private int _nxTotalLength;
        private int _calcLength;
        private int _nxStart;
        private bool _mirrorPart;

        private GreenTensor _asymGreenTensor;
        private GreenTensor _symmGreenTensor;

        public AtoAGreenTensorCalculator(
            ILogger logger,
            OmegaModel model,
            INativeMemoryProvider memoryProvider) :
            base(logger, model, memoryProvider)
        {
        }

        public void SetNxSizes(int nxStart, int nxTotalLength, int nxCalcLength)
        {
            _nxTotalLength = nxTotalLength;
            _nxStart = nxStart;

            _calcLength = nxCalcLength;
        }

        public GreenTensor CalculateAsymAtoA(ScalarSegments segments, MemoryLayoutOrder layoutOrder, bool mirrorPart)
        {
            if (segments == null) throw new ArgumentNullException(nameof(segments));
            _mirrorPart = mirrorPart;
            SetSegments(segments);
            _asymGreenTensor = AllocateNewAsym("xz", "yz");
            SetGreenTensorAndRadii(_asymGreenTensor, segments.Radii);

            if (_calcLength != 0)
            {
                if (!KnotsAreReady)
                    PrepareKnotsAtoA(segments.Radii, _nxStart, _calcLength);

                PrepareValuesForAsymAtoA(layoutOrder);
                SetSegments(segments);

                RunAlongXElectric(_nxStart, _calcLength);
                RunAlongYElectric(_nxStart, _calcLength);
            }
            return _asymGreenTensor;
        }

        public GreenTensor CalculateSymmAtoA(ScalarSegments segments, MemoryLayoutOrder layoutOrder, bool mirrorPart)
        {
            if (segments == null) throw new ArgumentNullException(nameof(segments));
            _mirrorPart = mirrorPart;
            SetSegments(segments);
            _symmGreenTensor = AllocateNewSymm("xx", "yy", "zz", "xy");
            SetGreenTensorAndRadii(_symmGreenTensor, segments.Radii);

            if (_calcLength != 0)
            {
                if (!KnotsAreReady)
                    PrepareKnotsAtoA(segments.Radii, _nxStart, _calcLength);

                PrepareValuesForSymmAtoA(layoutOrder);
                

                RunAlongXElectric(_nxStart, _calcLength);
                RunAlongYElectric(_nxStart, _calcLength);
            }

            return _symmGreenTensor;
        }

        private GreenTensor AllocateNewAsym(params string[] asym)
        {
            int compSize = (_nxTotalLength * 2 * Ny * Nz * Nz);
            var gt = GreenTensor.AllocateNew(MemoryProvider, _nxTotalLength, 2 * Ny, Nz, Nz, compSize, asym);
            return gt;
        }

        private GreenTensor AllocateNewSymm(params string[] symm)
        {
            var ny2 = 2 * Ny;
            int compSize = (_nxTotalLength * ny2 * (Nz + Nz * (Nz - 1) / 2));
            return GreenTensor.AllocateNew(MemoryProvider, _nxTotalLength, ny2, Nz, Nz, compSize, symm);
        }

        protected override int TransformI(int i)
            => _mirrorPart ? _nxTotalLength - i + _nxStart - 1 : i - _nxStart;

        private void RunAlongXElectric(int nxStart, int nxLength) =>
            RunAlongXElectric(leftX: nxStart, rightX: nxStart + nxLength, xShift: 0,
                              leftY: 0, rightY: Ny, yShift: 0);

        private void RunAlongYElectric(int nxStart, int nxLength) =>
            RunAlongYElectric(leftX: nxStart, rightX: nxStart + nxLength, xShift: 0,
                              leftY: 0, rightY: Ny, yShift: 0);

        private void PrepareValuesForAsymAtoA(MemoryLayoutOrder layoutOrder)
        {
            SetCalculateAll(false);
            CalculateXz = true;
            CalculateYz = true;
            SetQBufferSize(Nz * Nz);
            PrepareLayoutOrder(layoutOrder, _nxTotalLength, 2 * Ny, Nz, Nz);
        }

        private void PrepareValuesForSymmAtoA(MemoryLayoutOrder layoutOrder)
        {
            SetCalculateAll(false);
            CalculateXx = true;
            CalculateXy = true;
            CalculateYy = true;
            CalculateZz = true;
            SetQBufferSize(Nz + Nz * (Nz - 1) / 2);
            PrepareLayoutOrderSymm(layoutOrder, _nxTotalLength, 2 * Ny, Nz);
        }

        private void PrepareKnotsAtoA(double[] radii, int nxStart, int nxLength)
               => PrepareKnots(leftX: nxStart, rightX: nxStart + nxLength, xShift: 0,
                               leftY: 0, rightY: Ny, yShift: 0, radii: radii);
    }
}
