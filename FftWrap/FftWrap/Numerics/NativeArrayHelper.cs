using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace FftWrap.Numerics
{
    public static class NativeArrayHelper
    {
        public static void ForEach<T>(this NativeArray<T> array, Action<T> func) where T : struct 
        {
            for (int i = 0; i < array.Length; i++)
                    func(array[i]);
        }

        public static void ForEach<T>(this NativeArray<T> array, Action<int, T> func) where T : struct 
        {
            for (int i = 0; i < array.Length; i++)
                func(i, array[i]);
        }

        public static void SetEach<T>(this NativeArray<T> array, T value) where T : struct 
        {
            for (int i = 0; i < array.Length; i++)
                array[i] = value;
        }
    }
}
