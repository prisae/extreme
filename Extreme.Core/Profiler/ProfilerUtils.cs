using System;
using System.Runtime.CompilerServices;

namespace Extreme.Core
{
    public static class ProfilerUtils
    {
        private class Disposable : IDisposable
        {
            private readonly IProfiler _profiler;
            private readonly int _code;

            public Disposable(IProfiler profiler, int code)
            {
                _profiler = profiler;
                _code = code;
            }

            public void Dispose()
            {
                _profiler.End(_code);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Start(this IProfiler profiler, ProfilerEvent profilerEvent)
        {
            profiler.Start((int)profilerEvent);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void End(this IProfiler profiler, ProfilerEvent profilerEvent)
        {
            profiler.End((int)profilerEvent);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IDisposable StartAuto(this IProfiler profiler, ProfilerEvent profilerEvent)
        {
            profiler.Start((int)profilerEvent);
            return new Disposable(profiler, (int)profilerEvent);
        }
    }
}
