using System.Numerics;

namespace Extreme.Cartesian
{
    public struct ComplexVector
    {
        public static ComplexVector Zero;

        public ComplexVector(Complex x, Complex y, Complex z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public ComplexVector(ComplexVector vector)
        {
            X = vector.X;
            Y = vector.Y;
            Z = vector.Z;
        }

        public Complex X { get; }
        public Complex Y { get; }
        public Complex Z { get; }
        
        public static ComplexVector operator +(ComplexVector v1, ComplexVector v2)
            => new ComplexVector(v1.X + v2.X, v1.Y + v2.Y, v1.Z + v2.Z);

        public override string ToString()
            => $"{X} {Y} {Z}";
    }
}
