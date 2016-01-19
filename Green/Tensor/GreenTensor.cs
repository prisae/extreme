using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using Extreme.Core;
using UNM = Extreme.Cartesian.Forward.UnsafeNativeMethods;

namespace Extreme.Cartesian.Green.Tensor
{
    public unsafe class GreenTensor : IDisposable
    {
        public static readonly GreenTensor Empty = new GreenTensor(0, 0, 0, 0);

        private readonly List<IntPtr> _basePtrs = new List<IntPtr>();
        private readonly INativeMemoryProvider _memoryProvider;
        private IReadOnlyDictionary<string, Component> _components = new Dictionary<string, Component>();

        private bool _isDisposed = false;

        public int Nx { get; }
        public int Ny { get; }
        public int NTr { get; }
        public int NRc { get; }

        private GreenTensor(int nx, int ny, int nTr, int nRc)
        {
            Nx = nx;
            Ny = ny;
            NTr = nTr;
            NRc = nRc;
        }

        private GreenTensor(INativeMemoryProvider memoryProvider, int nx, int ny, int nTr, int nRc)
        {
            if (memoryProvider == null) throw new ArgumentNullException(nameof(memoryProvider));

            _memoryProvider = memoryProvider;
            Nx = nx;
            Ny = ny;
            NTr = nTr;
            NRc = nRc;
        }

        public static GreenTensor Merge(GreenTensor gt1, GreenTensor gt2)
        {
            if (gt1 == Empty)
                return gt2;

            if (gt2 == Empty)
                return gt1;

            var newGt = new GreenTensor(gt1._memoryProvider, gt1.Nx, gt1.Ny, gt1.NTr, gt1.NRc);

            newGt._components = Merge(gt1._components, gt2._components);
            newGt._basePtrs.AddRange(gt1._basePtrs);
            newGt._basePtrs.AddRange(gt2._basePtrs);

            return newGt;
        }

        private static Dictionary<T1, T2> Merge<T1, T2>(IReadOnlyDictionary<T1, T2> first, IReadOnlyDictionary<T1, T2> second)
        {
            if (first == null) throw new ArgumentNullException(nameof(first));
            if (second == null) throw new ArgumentNullException(nameof(second));

            var merged = new Dictionary<T1, T2>();
            first.ToList().ForEach(kv => merged[kv.Key] = kv.Value);
            second.ToList().ForEach(kv => merged[kv.Key] = kv.Value);

            return merged;
        }

        public static GreenTensor AllocateNew(INativeMemoryProvider memoryProvider,
        int nx, int ny, int nTr, int nRc, long compSize, params string[] components)
        {
            var gt = new GreenTensor(memoryProvider, nx, ny, nTr, nRc);
            var fullSize = compSize * components.Length;

            var ptr = memoryProvider.AllocateComplex(fullSize);
            UNM.ClearBuffer(ptr, fullSize);
	    

            var dict = new Dictionary<string, Component>();

            for (int i = 0; i < components.Length; i++)
            {
                var nextPtr = ptr + i * compSize;
                dict.Add(components[i], new Component(gt, nextPtr));
            }

            gt._basePtrs.Add(new IntPtr(ptr));
            gt._components = dict;

            return gt;
        }

        public static GreenTensor ReShape(GreenTensor gt, int nx, int ny, int nTr, int nRc)
        {
            return new GreenTensor(nx, ny, nTr, nRc)
            {
                _components = gt._components,
            };
        }

        public bool Has(string component)
            => _components.ContainsKey(component.ToLower());

        public Component this[string component]
        {
            get
            {
                var comp = component.ToLower();

                if (_components.ContainsKey(comp))
                    return _components[comp];

                throw new ArgumentOutOfRangeException(nameof(component), $"There is no such component ({component}) in the green tensor");
            }
        }

        public IEnumerable<string> GetAvailableComponents() => _components.Keys;

        public class Component
        {
            public Complex* Ptr { get; }
            private readonly GreenTensor _gt;

            public Component(GreenTensor gt, Complex* ptr)
            {
                Ptr = ptr;
                _gt = gt;
            }

            public Complex* this[int linearIndex]
                => Ptr + linearIndex;

            public Complex GetAlongVerticalAsym(int i, int j, int tr, int rc)
            {
                long index = ((long)i * _gt.Ny + j) * _gt.NTr * _gt.NRc + _gt.NTr * rc + tr;

                return Ptr[index];
            }


            public Complex GetAlongVerticalSymm(int i, int j, int tr, int rc)
            {
                int k = rc;
                int m = tr;

                if (tr >= rc)
                {
                    k = tr;
                    m = rc;
                }
                int nz = _gt.NTr;

                int symmPart = k + m * (m + 1) / 2;
                int size = nz + nz * (nz - 1) / 2;

                long index = ((long)i * _gt.Ny + j) * size + symmPart;

                return Ptr[index];
            }


            public Complex GetAlongLateralAsym(int i, int j, int tr, int rc)
            {
                long index = ((long)rc * _gt.NTr + tr) * _gt.Nx * _gt.Ny + i * _gt.Ny + j;

                return Ptr[index];
            }
        }

        public void Dispose()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(this.GetType().ToString());

            _basePtrs.ForEach(ptr => _memoryProvider?.ReleaseMemory(ptr));
            _components = null;

            _isDisposed = true;
        }
    }
}
