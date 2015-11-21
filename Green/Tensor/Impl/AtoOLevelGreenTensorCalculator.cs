using System;
using Extreme.Cartesian.Green.Scalar;
using Extreme.Cartesian.Model;
using Extreme.Core;
using Extreme.Cartesian.Green.Tensor;

namespace Extreme.Cartesian.Green.Tensor.Impl
{
    public class AtoOLevelGreenTensorCalculator : GreenTensorCalculator
    {
        public AtoOLevelGreenTensorCalculator(ILogger logger, OmegaModel model, INativeMemoryProvider memoryProvider) :
            base(logger, model, memoryProvider)
        {
        }


        private TensorPlan _plan;
        private bool _useLocalTransofrmForI = true;

        public GreenTensor CalculateAtoOElectric(ScalarSegments segments, MemoryLayoutOrder layoutOrder, TensorPlan plan)
        {
            if (segments == null) throw new ArgumentNullException(nameof(segments));
            _plan = plan;
            _useLocalTransofrmForI = _plan.NxTotalLength == 2 * Model.Nx;

            var greenTensor = AllocateNew("xx", "yy", "zz", "xy", "xz", "yz", "zx", "zy");
            SetSegments(segments);
            SetGreenTensorAndRadii(greenTensor, segments.Radii);

            if (_plan.NxCalcLength != 0)
            {
                PrepareValuesForAtoOElectricLevel(layoutOrder);
                PrepareKnotsAtoO(segments.Radii, _plan.NxStart, _plan.NxCalcLength);

                RunAlongXElectric(_plan.NxStart, _plan.NxCalcLength);
                RunAlongYElectric(_plan.NxStart, _plan.NxCalcLength);
            }

            return greenTensor;
        }

        public GreenTensor CalculateAtoOMagnetic(ScalarSegments segments, MemoryLayoutOrder layoutOrder, TensorPlan plan)
        {
            if (segments == null) throw new ArgumentNullException(nameof(segments));
            _plan = plan;
            _useLocalTransofrmForI = _plan.NxTotalLength == 2 * Model.Nx;

            var greenTensor = AllocateNew("xx", "yx", "xy", "xz", "yz", "zx", "zy");
            SetSegments(segments);
            SetGreenTensorAndRadii(greenTensor, segments.Radii);

            if (_plan.NxCalcLength != 0)
            {
                PrepareValuesForAtoOMagneticLevel(layoutOrder);
                PrepareKnotsAtoO(segments.Radii, _plan.NxStart, _plan.NxCalcLength);

                RunAlongXMagnetic(_plan.NxStart, _plan.NxCalcLength);
                RunAlongYMagnetic(_plan.NxStart, _plan.NxCalcLength);
            }

            return greenTensor;
        }

        private GreenTensor AllocateNew(params string[] components)
        {
            int compSize = (_plan.NxTotalLength * 2 * Ny * _plan.NTr * _plan.NRc);
            return GreenTensor.AllocateNew(MemoryProvider, _plan.NxTotalLength, 2 * Ny, _plan.NTr, _plan.NRc, compSize, components);
        }

        private void PrepareValuesForAtoOElectricLevel(MemoryLayoutOrder layoutOrder)
        {
            SetCalculateAll(true);
            CalculateYx = false;
            SetQBufferSize(_plan.NTr * _plan.NRc);
            PrepareLayoutOrder(layoutOrder, _plan.NxTotalLength, 2 * Ny, _plan.NTr, _plan.NRc);
        }

        private void PrepareValuesForAtoOMagneticLevel(MemoryLayoutOrder layoutOrder)
        {
            SetCalculateAll(true);
            CalculateZz = false;
            CalculateYy = false;
            SetQBufferSize(_plan.NTr * _plan.NRc);
            PrepareLayoutOrder(layoutOrder, _plan.NxTotalLength, 2 * Ny, _plan.NTr, _plan.NRc);
        }

        protected override int TransformI(int i)
        {
            if (_useLocalTransofrmForI)
                return i < 0 ? 2 * Nx + i % (2 * Nx) : i;

            return i - _plan.NxStart + (_plan.NxTotalLength - _plan.NxCalcLength);
        }

        protected override int TransformJ(int j)
            => j < 0 ? 2 * Ny + j % (2 * Ny) : j;

        private void RunAlongYElectric(int startX, int lengthX) =>
                RunAlongYElectric(leftX: startX, rightX: startX + lengthX, xShift: GetXShift(),
                leftY: -Ny + 1, rightY: Ny, yShift: GetYShift());

        private void RunAlongYMagnetic(int startX, int lengthX) =>
               RunAlongYMagnetic(leftX: startX, rightX: startX + lengthX, xShift: GetXShift(),
                                 leftY: -Ny + 1, rightY: Ny, yShift: GetYShift());

        private void RunAlongXElectric(int startX, int lengthX) =>
           RunAlongXElectric(leftX: startX, rightX: startX + lengthX, xShift: GetXShift(),
                             leftY: -Ny + 1, rightY: Ny, yShift: GetYShift());

        private void RunAlongXMagnetic(int startX, int lengthX) =>
            RunAlongXMagnetic(leftX: startX, rightX: startX + lengthX, xShift: GetXShift(),
                              leftY: -Ny + 1, rightY: Ny, yShift: GetYShift());

        private void PrepareKnotsAtoO(double[] radii, int startX, int lengthX)
        {
            PrepareKnots(leftX: startX, rightX: startX + lengthX, xShift: GetXShift(),
                         leftY: -Ny + 1, rightY: Ny, yShift: GetYShift(), radii: radii);
        }

        private double GetXShift()
            => -(double)_plan.ShiftX;

        private double GetYShift()
            => -(double)_plan.ShiftY;
    }
}
