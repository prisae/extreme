//using System;
//using System.Numerics;

//using Porvem.Cartesian.Core;
//using Porvem.Cartesian.Green.Tensor;

//namespace Extreme.Cartesian.Forward
//{
//    public unsafe class SiteConvolutor
//    {
//        private readonly GreenTensor _greenTensor;
//        private readonly AnomalyCurrent _jQ;
//        private readonly int _layerSize;

//        public SiteConvolutor(GreenTensor greenTensor, AnomalyCurrent jQ)
//        {
//            if (greenTensor == null) throw new ArgumentNullException(nameof(greenTensor));
//            if (jQ == null) throw new ArgumentNullException(nameof(jQ));

//            _greenTensor = greenTensor;
//            _jQ = jQ;
//            _layerSize = jQ.Nx * jQ.Ny;
//        }

//        public ComplexVector CalculateJtoE(int rc)
//        {
//            var x = Complex.Zero;
//            var y = Complex.Zero;
//            var z = Complex.Zero;

//            for (int tr = 0; tr < _jQ.Nz; tr++)
//            {
//                int greenShift = (rc * _jQ.Nz + tr) * _layerSize;
//                int srcShift = tr * _layerSize;

//                x += CalculateDotProductNotConjugated(_greenTensor["xx"].Ptr, _jQ.PtrX, greenShift, srcShift);
//                x += CalculateDotProductNotConjugated(_greenTensor["xy"].Ptr, _jQ.PtrY, greenShift, srcShift);
//                x -= CalculateDotProductNotConjugated(_greenTensor["xz"].Ptr, _jQ.PtrZ, greenShift, srcShift);
                                                                              
//                y += CalculateDotProductNotConjugated(_greenTensor["xy"].Ptr, _jQ.PtrX, greenShift, srcShift);
//                y += CalculateDotProductNotConjugated(_greenTensor["yy"].Ptr, _jQ.PtrY, greenShift, srcShift);
//                y -= CalculateDotProductNotConjugated(_greenTensor["yz"].Ptr, _jQ.PtrZ, greenShift, srcShift);
                                                                              
//                z -= CalculateDotProductNotConjugated(_greenTensor["zx"].Ptr, _jQ.PtrX, greenShift, srcShift);
//                z -= CalculateDotProductNotConjugated(_greenTensor["zy"].Ptr, _jQ.PtrY, greenShift, srcShift);
//                z += CalculateDotProductNotConjugated(_greenTensor["zz"].Ptr, _jQ.PtrZ, greenShift, srcShift);
//            }

//            return new ComplexVector(x, y, z);
//        }

//        public ComplexVector CalculateJtoH(int rc)
//        {
//            var x = Complex.Zero;
//            var y = Complex.Zero;
//            var z = Complex.Zero;

//            for (int tr = 0; tr < _jQ.Nz; tr++)
//            {
//                int greenShift = (rc * _jQ.Nz + tr) * _layerSize;
//                int srcShift = tr * _layerSize;

//                x -= CalculateDotProductNotConjugated(_greenTensor["xx"].Ptr, _jQ.PtrX, greenShift, srcShift);
//                x -= CalculateDotProductNotConjugated(_greenTensor["xy"].Ptr, _jQ.PtrY, greenShift, srcShift);
//                x += CalculateDotProductNotConjugated(_greenTensor["xz"].Ptr, _jQ.PtrZ, greenShift, srcShift);

//                y += CalculateDotProductNotConjugated(_greenTensor["yx"].Ptr, _jQ.PtrX, greenShift, srcShift);
//                y += CalculateDotProductNotConjugated(_greenTensor["xx"].Ptr, _jQ.PtrY, greenShift, srcShift);
//                y -= CalculateDotProductNotConjugated(_greenTensor["yz"].Ptr, _jQ.PtrZ, greenShift, srcShift);

//                z += CalculateDotProductNotConjugated(_greenTensor["zx"].Ptr, _jQ.PtrX, greenShift, srcShift);
//                z -= CalculateDotProductNotConjugated(_greenTensor["zy"].Ptr, _jQ.PtrY, greenShift, srcShift);
//            }

//            return new ComplexVector(x, y, z);
//        }

//        private Complex CalculateDotProductNotConjugated(Complex* gt, IntPtr src, int greenShift, int srcShift)
//        {
//            var result = Complex.Zero;
//            var srcPtr = (Complex*) src.ToPointer();
//            UnsafeNativeMethods.CalculateProductNotConjugatedDouble(_layerSize, gt + greenShift, srcPtr + srcShift, ref result);
//            return result;
//        }
//    }
//}
