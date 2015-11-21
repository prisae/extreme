using System;
using System.Numerics;

namespace Extreme.Cartesian.Fft
{
    public unsafe class UnsafeArray
    {
        private int _dim3nx;
        private int _dim3ny;
        private int _dim3nz;

        private int _dim2nxy;
        private int _dim2nz;

        private readonly Complex* _data;

        public Complex* Ptr => _data;
        public IntPtr IntPtr => new IntPtr(_data);

        public UnsafeArray(Complex* data)
        {
            _data = data;
        }

        public UnsafeArray ReShape(int nx, int ny, int nz)
        {
            _dim3nx = nx;
            _dim3ny = ny;
            _dim3nz = nz;

            return this;
        }

        public UnsafeArray ReShape(int nxy, int nz)
        {
            _dim2nxy = nxy;
            _dim2nz = nz;

            return this;
        }

        public Complex this[int i]
        {
            get { return _data[i]; }
            set { _data[i] = value; }
        }

        public Complex this[int i, int j]
        {
            get { return _data[i * _dim2nz + j]; }
            set { _data[i * _dim2nz + j] = value; }
        }

        public Complex this[int i, int j, int k]
        {
            get { return _data[(i * _dim3ny + j) * _dim3nz + k]; }
            set { _data[(i * _dim3ny + j) * _dim3nz + k] = value; }
        }
    }
}
