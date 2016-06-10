using System;
using System.Runtime.InteropServices;

namespace FftWrap.Numerics
{
    [StructLayout(LayoutKind.Sequential)]
    public static class NativeMatrixHelper
    {
        public static void ForEach<T>(this NativeMatrix<T> matrix, Action<T> func) where T : struct
        {
            for (int i = 0; i < matrix.Nx; i++)
                for (int j = 0; j < matrix.Ny; j++)
                    func(matrix[i,j]);
        }

        public static void ForEach<T>(this NativeMatrix<T> matrix, Action<int, int, T> func) where T : struct
        {
            for (int i = 0; i < matrix.Nx; i++)
                for (int j = 0; j < matrix.Ny; j++)
                    func(i, j, matrix[i,j]);
        }

        public static void ChangeEach<T>(this NativeMatrix<T> matrix, Func<T, T> func) where T : struct
        {
            for (int i = 0; i < matrix.Nx; i++)
                for (int j = 0; j < matrix.Ny; j++)
                    matrix[i, j] = func(matrix[i, j]);
        }

        public static void ChangeEach<T>(this NativeMatrix<T> matrix, Func<int, int, T, T> func) where T : struct
        {
            for (int i = 0; i < matrix.Nx; i++)
                for (int j = 0; j < matrix.Ny; j++)
                    matrix[i, j]=func(i, j, matrix[i, j]);
        }

        public static void SetEach<T>(this NativeMatrix<T> matrix, T value) where T : struct
        {
            for (int i = 0; i < matrix.Nx; i++)
                for (int j = 0; j < matrix.Ny; j++)
                    matrix[i, j] = value;
        }
    }
}
