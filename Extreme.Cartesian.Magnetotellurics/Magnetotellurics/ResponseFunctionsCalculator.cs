//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
﻿using Extreme.Cartesian;
using Extreme.Cartesian.Magnetotellurics;

namespace Porvem.Magnetotellurics
{
    public class ResponseFunctionsCalculator
    {
        public static ImpedanceTensor CalculateImpedanceTensor(AllFieldsAtSite site)
        {
            var tensor = CalculateTensor(site.FullE1, site.FullE2, site.FullH1, site.FullH2);
            return new ImpedanceTensor(tensor.Xx, tensor.Xy, tensor.Yx, tensor.Yy);
        }

        public static ImpedanceTensor CalculateImpedanceTensor(ComplexVector e1, ComplexVector e2, ComplexVector h1, ComplexVector h2)
        {
            var tensor = CalculateTensor(e1, e2, h1, h2);

            return new ImpedanceTensor(tensor.Xx, tensor.Xy, tensor.Yx, tensor.Yy);
        }

        public static ElectricTensor CalculateElectricTensor(ComplexVector e1, ComplexVector e2, ComplexVector e1Base, ComplexVector e2Base)
        {
            var tensor = CalculateTensor(e1, e2, e1Base, e2Base);

            return new ElectricTensor(tensor.Xx, tensor.Xy, tensor.Yx, tensor.Yy);
        }

        public static QuasiElectricTensor CalculateQuasiElectricTensor(ComplexVector e1, ComplexVector e2, ComplexVector h1Base, ComplexVector h2Base)
        {
            var tensor = CalculateTensor(e1, e2, h1Base, h2Base);

            return new QuasiElectricTensor(tensor.Xx, tensor.Xy, tensor.Yx, tensor.Yy);
        }

        private static Tensor CalculateTensor(ComplexVector f11, ComplexVector f12, ComplexVector f21, ComplexVector f22)
        {
            var determinant = f21.X * f22.Y - f22.X * f21.Y;

            var hi11 = f22.Y / determinant;
            var hi12 = -f22.X / determinant;
            var hi21 = -f21.Y / determinant;
            var hi22 = f21.X / determinant;

            var zxx = f11.X * hi11 + f12.X * hi21;
            var zxy = f11.X * hi12 + f12.X * hi22;
            var zyx = f11.Y * hi11 + f12.Y * hi21;
            var zyy = f11.Y * hi12 + f12.Y * hi22;

            return new ImpedanceTensor(zxx, zxy, zyx, zyy);
        }

        public static ImpedancePhaseTensor CalculateImpedancePhaseTensor(ImpedanceTensor z)
        {
            var phaseTensor = CalculatePhaseTensor(z);

            return new ImpedancePhaseTensor(phaseTensor.Xx, phaseTensor.Xy, phaseTensor.Yx, phaseTensor.Yy);
        }

        public static ElectricPhaseTensor CalculateElectricPhaseTensor(ElectricTensor z)
        {
            var phaseTensor = CalculatePhaseTensor(z);

            return new ElectricPhaseTensor(phaseTensor.Xx, phaseTensor.Xy, phaseTensor.Yx, phaseTensor.Yy);
        }

        public static QuasiElectricPhaseTensor CalculateQuasiElectricPhaseTensor(QuasiElectricTensor z)
        {
            var phaseTensor = CalculatePhaseTensor(z);

            return new QuasiElectricPhaseTensor(phaseTensor.Xx, phaseTensor.Xy, phaseTensor.Yx, phaseTensor.Yy);
        }

        public static PhaseTensor CalculatePhaseTensor(Tensor tensor)
        {
            var determinant = tensor.Xx.Real * tensor.Yy.Real - tensor.Xy.Real * tensor.Yx.Real;

            var xx = (tensor.Yy.Real * tensor.Xx.Imaginary - tensor.Xy.Real * tensor.Yx.Imaginary) / determinant;
            var xy = (tensor.Yy.Real * tensor.Xy.Imaginary - tensor.Xy.Real * tensor.Yy.Imaginary) / determinant;
            var yx = (tensor.Xx.Real * tensor.Yx.Imaginary - tensor.Yx.Real * tensor.Xx.Imaginary) / determinant;
            var yy = (tensor.Xx.Real * tensor.Yy.Imaginary - tensor.Yx.Real * tensor.Xy.Imaginary) / determinant;

            return new PhaseTensor(xx, xy, yx, yy);
        }

        public static MagneticTipper CalculateTipper(AllFieldsAtSite site)
        {
            var tipper = CalculateTipper(site.FullH1, site.FullH2);
            return new MagneticTipper(tipper.Zx, tipper.Zy);
        }

        public static ElectricTipper CalculateElectricTipper(AllFieldsAtSite site)
        {
            var tipper = CalculateTipper(site.FullE1, site.FullE2);
            return new ElectricTipper(tipper.Zx, tipper.Zy);
        }

        public static Tipper CalculateTipper(ComplexVector h1, ComplexVector h2)
        {
            var determinant = h1.X * h2.Y - h2.X * h1.Y;

            var hi11 = h2.Y / determinant;
            var hi12 = -h2.X / determinant;
            var hi21 = -h1.Y / determinant;
            var hi22 = h1.X / determinant;

            var wzx = h1.Z * hi11 + h2.Z * hi21;
            var wzy = h1.Z * hi12 + h2.Z * hi22;

            return new Tipper(wzx, wzy);
        }
    }
}
