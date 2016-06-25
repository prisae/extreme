//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
﻿using System.Globalization;

namespace Extreme.Core
{
    public struct Size2D
    {
        public static readonly Size2D Empty = default(Size2D);

        private readonly int _nx;
        private readonly int _ny;

        public Size2D(int nx, int ny)
        {
            _nx = nx;
            _ny = ny;
        }

        public int Nx { get { return _nx; } }
        public int Ny { get { return _ny; } }

        public static bool operator ==(Size2D left, Size2D right)
        {
            return left.Nx == right.Nx && left.Ny == right.Ny;
        }

        public static bool operator !=(Size2D left, Size2D right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Size2D))
            {
                return false;
            }

            var point = (Size2D) obj;
            
            return point.Nx == this.Nx && point.Ny == this.Ny;
        }

        public override int GetHashCode()
        {
            return Nx.GetHashCode() ^ Ny.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{{x={0}, y={1}}}", Nx, Ny);
        }
    }
}
