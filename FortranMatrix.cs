using System;
using System.Numerics;
using Extreme.Core;

namespace Extreme.Fgmres
{
    /// <summary>
    /// Column-major order native matrix
    /// </summary>
    public unsafe class FortranMatrix : IDisposable
    {
        private readonly INativeMemoryProvider _memoryProvider;
        public int Nx { get; }
        public int Ny { get; }

        private readonly Complex* _ptr;

        public FortranMatrix(INativeMemoryProvider memoryProvider, int nx, int ny)
        {
            _ptr = memoryProvider.AllocateComplex(nx * ny);
            Nx = nx;
            Ny = ny;
            _memoryProvider = memoryProvider;
        }

        public Complex this[int i, int j]
        {
            get { return _ptr[j * Nx + i]; }
            set { _ptr[j * Nx + i] = value; }
        }
        
        public Complex* GetPointer(int i, int j)
            => _ptr + j * Nx + i;

        public Complex* Ptr => _ptr;

        public Complex* GetColumn(int j)
             => _ptr + j * Nx;

        public NativeVector GetColumnVector(int i)
             => new NativeVector(GetColumn(i), Nx);

        public void Dispose()
            => _memoryProvider.Release(_ptr);

        public void FillAll(Complex value)
        {
            for (int i = 0; i < Nx * Ny; i++)
                _ptr[i] = value;
        }
    }
}
