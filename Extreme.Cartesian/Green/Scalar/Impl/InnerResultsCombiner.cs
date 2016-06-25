//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
﻿using System.Collections.Generic;
using Extreme.Core;
using Extreme.Cartesian.Green.Scalar;

namespace Extreme.Cartesian.Green.Scalar.Impl
{
    public class InnerResultsCombiner
    {
        private readonly ScalarPlan _plan;
        private readonly SortedList<double, AuxIndecies> _sortedIndecies;

        private InnerResultsCombiner(ScalarPlan plan, SortedList<double, AuxIndecies> sortedIndecies)
        {
            _plan = plan;
            _sortedIndecies = sortedIndecies;
        }

        public static InnerResultsCombiner CreateFromSample(ScalarPlan plan, InnerResult[] sample)
        {
            var sorted = new SortedList<double, AuxIndecies>();

            for (int i = 0; i < sample.Length; i++)
            {
                var rho = sample[i].Rho;

                for (int j = 0; j < rho.Length; j++)
                {
                    if (!sorted.ContainsKey(rho[j]))
                        sorted.Add(rho[j], new AuxIndecies(i, j));
                }
            }

            return new InnerResultsCombiner(plan, sorted);
        }

        public SingleGreenScalar[] CombineSeparatedScalars(Transceiver[] trans, InnerResult[][] separatedResults)
        {
            var result = new SingleGreenScalar[trans.Length];

            for (int i = 0; i < result.Length; i++)
                result[i] = CombineSeparatedScalars(trans[i], separatedResults[i]);

            return result;
        }

        public SingleGreenScalar CombineSeparatedScalars(Transceiver transceiver, InnerResult[] separatedResults)
        {
            int length = _sortedIndecies.Count;

            var result = new SingleGreenScalar(_plan, transceiver, length);

            for (int k = 0; k < length; k++)
            {
                var sampleIndex = _sortedIndecies.Values[k].SampleIndex;
                var valueIndex = _sortedIndecies.Values[k].RhoIndex;

                CopyScalarResultValues(valueIndex, k, separatedResults[sampleIndex], result);
            }

            return result;
        }

        private class AuxIndecies
        {
            public readonly int SampleIndex;
            public readonly int RhoIndex;
            public AuxIndecies(int sampleIndex, int rhoIndex)
            {
                SampleIndex = sampleIndex;
                RhoIndex = rhoIndex;
            }
        }

        private static void CopyScalarResultValues(int srcIndex, int dstIndex, InnerResult src, SingleGreenScalar dst)
        {
            if (src.I1.Length != 0)
                dst.I1[dstIndex] = src.I1[srcIndex];

            if (src.I2.Length != 0)
                dst.I2[dstIndex] = src.I2[srcIndex];

            if (src.I3.Length != 0)
                dst.I3[dstIndex] = src.I3[srcIndex];

            if (src.I4.Length != 0)
                dst.I4[dstIndex] = src.I4[srcIndex];

            if (src.I5.Length != 0)
                dst.I5[dstIndex] = src.I5[srcIndex];
        }
    }
}
