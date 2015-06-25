using System;
using FftWrap.Numerics;

namespace FftWrap
{
    public abstract class Plan : IDisposable
    {
        private readonly IntPtr _planPtr;
        private Plan(IntPtr planPtr)
        {
            _planPtr = planPtr;
        }
        protected IntPtr PlanPtr
        {
            get { return _planPtr; }
        }

        public static Plan Create(NativeArray<SingleComplex> array, Direction direction)
        {
            IntPtr ptr = Fftw.PlanDft1D(array.Length, array.Ptr, array.Ptr, (int)direction, (uint)Flags.Estimate);

            return new ArrayPlan<SingleComplex>(ptr, array);
        }

        public abstract void Execute();
        public abstract void Execute<T>(NativeArray<T> array) where T : struct;

        private class ArrayPlan<T> : Plan where T : struct
        {
            private readonly NativeArray<SingleComplex> _array;

            public ArrayPlan(IntPtr planPtr, NativeArray<SingleComplex> array)
                : base(planPtr)
            {
                _array = array;
            }

            public override void Execute()
            {
                Fftw.Execute(PlanPtr);
            }

            public override void Execute<T>(NativeArray<T> array)
            {
                Fftw.ExecuteDft(PlanPtr, array.Ptr, array.Ptr);
            }
        }


        #region Dispose

        ~Plan()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                // release managed resources
            }

            ReleaseUnmanagedResources();
        }

        private void ReleaseUnmanagedResources()
        {
            Fftw.DestroyPlan(_planPtr);
        }

        #endregion

    }
}
