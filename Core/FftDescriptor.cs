using System;
using Extreme.Core;

namespace Extreme.Cartesian.Core
{
    public struct FftDescriptor
    {
        public static readonly FftDescriptor Empty = default(FftDescriptor);

        public FftDescriptor(int nx, int ny, IntPtr ptr)
        {
            Nx = nx;
            Ny = ny;
            Ptr = ptr;
        }

        public IntPtr Ptr { get; }
        public int Ny { get; }
        public int Nx { get; }

        public override bool Equals(object obj)
            => obj is FftDescriptor && this == (FftDescriptor)obj;

        public static bool operator ==(FftDescriptor d1, FftDescriptor d2)
            => d1.Nx == d2.Nx && d1.Ny == d2.Ny;

        public static bool operator !=(FftDescriptor d1, FftDescriptor d2)
            => !(d1 == d2);

        public override int GetHashCode()
            => Nx.GetHashCode() ^ Ny.GetHashCode();
    }
}
