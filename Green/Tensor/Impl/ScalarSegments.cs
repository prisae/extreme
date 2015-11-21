using System;
using System.Numerics;
using Extreme.Core;

namespace Extreme.Cartesian.Green.Scalar
{
    public unsafe class ScalarSegments : IDisposable
    {
        public Complex* Ptr { get; }

        private readonly INativeMemoryProvider _memoryProvider;

        public readonly double[] Radii;
        public readonly SingleScalarSegment[] SingleSegment;

        private ScalarSegments(INativeMemoryProvider memoryProvider, Complex* ptr, double[] radii, SingleScalarSegment[] segment)
        {
            _memoryProvider = memoryProvider;
            Ptr = ptr;
            Radii = radii;
            SingleSegment = segment;
        }

        private ScalarSegments(Complex* ptr, double[] radii, SingleScalarSegment[] segment)
        {
            Ptr = ptr;
            Radii = radii;
            SingleSegment = segment;
        }

        public static ScalarSegments AllocateAndConvert(INativeMemoryProvider memoryProvider, ScalarPlan plan, GreenScalars scalars)
        {
            var segments = AllocateScalarSegments(memoryProvider, plan, scalars);
            ConvertScalarsToSegments(scalars, segments);
            return segments;
        }

        private static ScalarSegments AllocateScalarSegments(INativeMemoryProvider memoryProvider, ScalarPlan plan, GreenScalars scalars)
            => AllocateScalarSegments(memoryProvider, plan, scalars, scalars.SingleScalars.Length);

        public static ScalarSegments AllocateScalarSegments(INativeMemoryProvider memoryProvider, ScalarPlan plan, GreenScalars scalars, int length)
        {
            var radii = scalars.Radii;
            int nRho = radii.Length - 1;
            var segments = new SingleScalarSegment[nRho];

            int nComp = scalars.GetNumberOfAvailableIs(plan);
            var ptr = memoryProvider.AllocateComplex(length * nComp * 2L * nRho);

            long oneSegmentSize = length * nComp * 2L;

            for (int i = 0; i < segments.Length; i++)
            {
                var startPtr = ptr + oneSegmentSize * i;
                segments[i] = new SingleScalarSegment(plan, startPtr, length, nComp);
            }

            return new ScalarSegments(memoryProvider, ptr, radii, segments);
        }

        public static ScalarSegments ReUseScalarSegments(Complex* ptr, ScalarPlan plan, GreenScalars scalars, int length)
        {
            var radii = scalars.Radii;
            int nComp = scalars.GetNumberOfAvailableIs(plan);
            int nRho = radii.Length - 1;

            var segments = new SingleScalarSegment[nRho];
            long oneSegmentSize = length * nComp * 2L;

            for (int i = 0; i < segments.Length; i++)
            {
                var startPtr = ptr + oneSegmentSize * i;
                segments[i] = new SingleScalarSegment(plan, startPtr, length, nComp);
            }

            return new ScalarSegments(ptr, radii, segments);
        }

        public static void ConvertScalarsToSegments(GreenScalars scalars, ScalarSegments segments)
        {
            for (int i = 0; i < segments.SingleSegment.Length; i++)
                CalculateScalarSegment(scalars, i, segments.SingleSegment[i]);
        }

        private static void CalculateScalarSegment(GreenScalars scalars, int index, SingleScalarSegment segment)
        {
            int length = scalars.SingleScalars.Length;

            var s = scalars.Radii[index];
            var u = scalars.Radii[index + 1];
            var div = (u - s);

            for (int i = 0; i < length; i++)
            {
                var sg = scalars.SingleScalars[i];

                if (segment.I1A != null)
                    segment.I1A[i] = (sg.I1[index + 1] - sg.I1[index]) / div;

                if (segment.I2A != null)
                    segment.I2A[i] = (sg.I2[index + 1] - sg.I2[index]) / div;

                if (segment.I3A != null)
                    segment.I3A[i] = (sg.I3[index + 1] - sg.I3[index]) / div;

                if (segment.I4A != null)
                    segment.I4A[i] = (sg.I4[index + 1] - sg.I4[index]) / div;

                if (segment.I5A != null)
                    segment.I5A[i] = (sg.I5[index + 1] - sg.I5[index]) / div;

                if (segment.I1B != null)
                    segment.I1B[i] = sg.I1[index] - segment.I1A[i] * s;

                if (segment.I2B != null)
                    segment.I2B[i] = sg.I2[index] - segment.I2A[i] * s;

                if (segment.I3B != null)
                    segment.I3B[i] = sg.I3[index] - segment.I3A[i] * s;

                if (segment.I4B != null)
                    segment.I4B[i] = sg.I4[index] - segment.I4A[i] * s;

                if (segment.I5B != null)
                    segment.I5B[i] = sg.I5[index] - segment.I5A[i] * s;
            }
        }

        public void Dispose()
        {
            _memoryProvider?.Release(Ptr);
        }
    }
}
