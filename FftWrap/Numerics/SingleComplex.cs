using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace FftWrap.Numerics
{
    [StructLayout(LayoutKind.Sequential)]
    public struct SingleComplex
    {
        /// <summary>Returns a new instance with a real number equal to zero and an imaginary number equal to zero.</summary>
        public static readonly SingleComplex Zero = new SingleComplex(0.0f, 0.0f);

        /// <summary>Returns a new instance with a real number equal to one and an imaginary number equal to zero.</summary>
        public static readonly SingleComplex One = new SingleComplex(1.0f, 0.0f);

        /// <summary>Returns a new  instance with a real number equal to zero and an imaginary number equal to one.</summary>
        public static readonly SingleComplex ImaginaryOne = new SingleComplex(0.0f, 1.0f);

        public static SingleComplex FromReal(float real)
        {
            return new SingleComplex(real, 0);
        }

        public static SingleComplex FromImaginary(float imaginary)
        {
            return new SingleComplex(0, imaginary);
        }

        private readonly float _real;
        private readonly float _imaginary;

        public SingleComplex(float real, float imaginary)
        {
            _real = real;
            _imaginary = imaginary;
        }

        public float Real { get { return _real; } }
        public float Imaginary { get { return _imaginary; } }
        
        public override string ToString()
        {
            return string.Format("({0}, {1})", _real, _imaginary);
        }

        public static explicit operator Complex(SingleComplex value)
        {
            return new Complex(value.Real, value.Imaginary);
        }

        public static explicit operator SingleComplex(Complex value)
        {
            return new SingleComplex((float)value.Real, (float)value.Imaginary);
        }

        public static implicit operator SingleComplex(float value)
        {
            return new SingleComplex(value, 0);
        }

        public static SingleComplex operator -(SingleComplex value)
        {
            return new SingleComplex(-value.Real, -value.Imaginary);
        }

        public static SingleComplex operator +(SingleComplex left, SingleComplex right)
        {
            return new SingleComplex(left.Real + right.Real, left.Imaginary + right.Imaginary);
        }

        public static SingleComplex operator -(SingleComplex left, SingleComplex right)
        {
            return new SingleComplex(left.Real - right.Real, left.Imaginary - right.Imaginary);
        }

        public static SingleComplex operator /(SingleComplex left, float right)
        {
            return new SingleComplex(left.Real / right, left.Imaginary / right);
        }

        public static SingleComplex operator /(float left, SingleComplex right)
        {
            var r1 = left;
            var r2 = right.Real;
            var i2 = right.Imaginary;

            if (Math.Abs(i2) < Math.Abs(r2))
            {
                var num1 = i2 / r2;
                return new SingleComplex((r1) / (r2 + i2 * num1), (-r1 * num1) / (r2 + i2 * num1));
            }

            var num2 = r2 / i2;
            return new SingleComplex((r1 * num2) / (i2 + r2 * num2), (-r1) / (i2 + r2 * num2));
        }

        public static SingleComplex operator /(SingleComplex left, SingleComplex right)
        {
            var r1 = left.Real;
            var i1 = left.Imaginary;
            var r2 = right.Real;
            var i2 = right.Imaginary;

            if (Math.Abs(i2) < Math.Abs(r2))
            {
                var num1 = i2 / r2;
                return new SingleComplex((r1 + i1 * num1) / (r2 + i2 * num1), (i1 - r1 * num1) / (r2 + i2 * num1));
            }

            var num2 = r2 / i2;
            return new SingleComplex((i1 + r1 * num2) / (i2 + r2 * num2), (-r1 + i1 * num2) / (i2 + r2 * num2));
        }

        public static SingleComplex operator *(SingleComplex left, float right)
        {
            return new SingleComplex(left.Real * right, left.Imaginary * right);
        }

        public static SingleComplex operator *(float left, SingleComplex right)
        {
            return new SingleComplex(left * right.Real, left * right.Imaginary);
        }

        public static SingleComplex operator *(SingleComplex left, SingleComplex right)
        {
            return new SingleComplex(left.Real * right.Real - left.Imaginary * right.Imaginary, left.Imaginary * right.Real + left.Real * right.Imaginary);
        }

        public static SingleComplex Conjugate(SingleComplex value)
        {
            return new SingleComplex(value.Real, -value.Imaginary);
        }
    }
}
