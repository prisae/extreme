//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
﻿using System;
using Extreme.Cartesian.Green.Scalar;
using Extreme.Cartesian.Model;
using Extreme.Core;
using Extreme.Cartesian.Green.Tensor;

namespace Extreme.Cartesian.Green.Tensor.Impl
{
    public  class AtoOSiteGreenTensorCalculator : GreenTensorCalculator
    {
        public AtoOSiteGreenTensorCalculator(
            ILogger logger,
            OmegaModel model,
            INativeMemoryProvider memoryProvider) :
            base(logger, model, memoryProvider)
        {
        }

        public GreenTensor CalculateAtoOElectric(ScalarSegments segments, ObservationSite site, MemoryLayoutOrder layoutOrder)
        {
            if (segments == null) throw new ArgumentNullException(nameof(segments));

            var greenTensor = AllocateNew("xx", "yy", "zz", "xy", "xz", "yz", "zx", "zy");
            SetSegments(segments);
            SetGreenTensorAndRadii(greenTensor, segments.Radii);

            PrepareValuesForAtoOElectricSite(layoutOrder);
            PrepareKnotsAtoO(segments.Radii, site);
            
            RunAlongXElectric(site);
            RunAlongYElectric(site);

            return greenTensor;
        }

        public GreenTensor CalculateAtoOMagnetic(ScalarSegments segments, ObservationSite site, MemoryLayoutOrder layoutOrder)
        {
            if (segments == null) throw new ArgumentNullException(nameof(segments));
            
            var greenTensor = AllocateNew("xx", "yx", "xy", "xz", "yz", "zx", "zy");
            SetSegments(segments);
            SetGreenTensorAndRadii(greenTensor, segments.Radii);

            PrepareValuesForAtoOMagneticSite(layoutOrder);
            PrepareKnotsAtoO(segments.Radii, site);

            RunAlongXMagnetic(site);
            RunAlongYMagnetic(site);

            return greenTensor;
        }

        private GreenTensor AllocateNew(params string[] components)
        {
            int compSize = (Nx * Ny * Nz);
            return GreenTensor.AllocateNew(MemoryProvider, Nx, Ny, Nz, 1, compSize, components);
        }

        private void PrepareValuesForAtoOElectricSite(MemoryLayoutOrder layoutOrder)
        {
            SetCalculateAll(true);
            CalculateYx = false;
            SetQBufferSize(Nz);
            PrepareLayoutOrder(layoutOrder, Nx, Ny, Nz, 1);
        }

        private void PrepareValuesForAtoOMagneticSite(MemoryLayoutOrder layoutOrder)
        {
            SetCalculateAll(true);
            CalculateZz = false;
            CalculateYy = false;
            SetQBufferSize(Nz);
            PrepareLayoutOrder(layoutOrder, Nx, Ny, Nz, 1);
        }

    
        private void RunAlongYElectric(ObservationSite site) =>
               RunAlongYElectric(leftX: 0, rightX: Nx, xShift: GetXShift(site),
                                 leftY: 0, rightY: Ny, yShift: GetYShift(site));
        
        private void RunAlongYMagnetic(ObservationSite site) =>
               RunAlongYMagnetic(leftX: 0, rightX: Nx, xShift: GetXShift(site),
                                 leftY: 0, rightY: Ny, yShift: GetYShift(site));

        private void RunAlongXElectric(ObservationSite site) =>
               RunAlongXElectric(leftX: 0, rightX: Nx, xShift: GetXShift(site),
                                 leftY: 0, rightY: Ny, yShift: GetYShift(site));

        private void RunAlongXMagnetic(ObservationSite site) =>
            RunAlongXMagnetic(leftX: 0, rightX: Nx, xShift: GetXShift(site),
                              leftY: 0, rightY: Ny, yShift: GetYShift(site));

        private void PrepareKnotsAtoO(double[] radii, ObservationSite site)
            => PrepareKnots(leftX: 0, rightX: Nx, xShift: GetXShift(site),
                            leftY: 0, rightY: Ny, yShift: GetYShift(site), radii: radii);

        private double GetXShift(ObservationSite site)
            => (double)(-Model.LateralDimensions.CellSizeX / 2 + site.X);

        private double GetYShift(ObservationSite site)
           => (double)(-Model.LateralDimensions.CellSizeY / 2 + site.Y);
    }
}
