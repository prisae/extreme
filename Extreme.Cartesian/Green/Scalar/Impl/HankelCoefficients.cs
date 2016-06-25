//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Security;
using System.Xml.Linq;
using Extreme.Core;

namespace Extreme.Cartesian.Green
{
    public class HankelCoefficients
    {
        private readonly double[] _hank0;
        private readonly double[] _hank1;
        private readonly float _ndec;

        private readonly int _l1 = -300;
        private readonly int _l2 = 130;

        private const int M1 = 0;

        private HankelCoefficients(float ndec, double[] hank0, double[] hank1, int l1, int l2)
        {
            _hank0 = hank0;
            _hank1 = hank1;
            _l1 = l1;
            _l2 = l2;
            _ndec = ndec;
        }

        public static HankelCoefficients LoadN15() => LoadFromResourcesForNdec(15, -300, 130);
        public static HankelCoefficients LoadN20() => LoadFromResourcesForNdec(20, -300, 130);
        public static HankelCoefficients LoadN30() => LoadFromResourcesForNdec(30, -300, 130);
        public static HankelCoefficients LoadN40() => LoadFromResourcesForNdec(40, -300, 130);
        public static HankelCoefficients LoadN60() => LoadFromResourcesForNdec(40, -600, 230);

        private static HankelCoefficients LoadFromResourcesForNdec(int ndec, int l1, int l2)
        {
            var xdoc = XDocument.Parse(HankelResources.hankelcoeff);

            var xHank = xdoc.Element(@"Hankel");

            if (xHank == null)
                throw new InvalidOperationException();

            var xelem = xHank.Elements(@"Coefficients")
                              .First(x => x.Attribute(@"ndec").Value == ndec.ToString(CultureInfo.InvariantCulture));

            var h0 = new List<double>();
            var h1 = new List<double>();

            foreach (var line in xelem.Elements(@"line"))
            {
                h0.Add(line.AttributeAsDouble(@"h0"));
                h1.Add(line.AttributeAsDouble(@"h1"));
            }

            var hank0 = h0.ToArray();
            var hank1 = h1.ToArray();

            return new HankelCoefficients(ndec, hank0, hank1, l1, l2);
        }

        public float GetLog10RhoStep() => (float)System.Math.Pow(10, 1 / _ndec);
        public int GetLengthOfLambdaWithRespectTo(int rhoLength) => _hank0.Length + rhoLength;
        public int GetN1WithRespectTo(int rhoLength) => M1 - _l2;

        public int GetN2WithRespectTo(int rhoLength)
        {
            var nrMax = GetLengthOfLambdaWithRespectTo(rhoLength);

            return nrMax - 1 - _l2;
        }

        public Complex[] ConvoluteWithHank0(Complex[] fk, int length) => ConvoluteWith(_hank0, fk, length);
        public Complex[] ConvoluteWithHank1(Complex[] fk, int length) => ConvoluteWith(_hank1, fk, length);
        public Complex[] ConvoluteWithHank0Fast(Complex[] fk, int length) => ConvoluteWithFast(_hank0, fk, length);
        public Complex[] ConvoluteWithHank1Fast(Complex[] fk, int length) => ConvoluteWithFast(_hank1, fk, length);


        private Complex[] ConvoluteWith(double[] hank, Complex[] fk, int length)
        {
            int ml = length - 1;
            int n1 = GetN1WithRespectTo(length);
            var fr = new Complex[ml + 1];

            for (int i = 0; i <= ml; i++)
            {
                var s = Complex.Zero;

                for (int j = 0; j < hank.Length; j++)
                    s += fk[i - j - (_l1 + n1)] * hank[j];

                fr[i] = s;
            }

            return fr;
        }

        private Complex[] ConvoluteWithFast(double[] hank, Complex[] fk, int length)
        {
            int ml = length - 1;
            int n1 = GetN1WithRespectTo(length);
            var fr = new Complex[ml + 1];

            var revHank = hank.Reverse().ToArray();

            for (int i = 0; i <= ml; i++)
            {
                int shift = i - (_l1 + n1) - revHank.Length + 1;
                fr[i] = DotProductNative(revHank, fk, shift);
            }

            return fr;
        }

        private static Complex DotProduct(double[] hank, Complex[] fk, int shift)
        {
            var s = Complex.Zero;

            for (int j = 0; j < hank.Length; j++)
                s += fk[shift + j] * hank[j];

            return s;
        }

        private static unsafe Complex DotProductNative(double[] hank, Complex[] fk, int shift)
        {
            fixed (Complex* fkPtr = &fk[0])
            fixed (double* hankPtr = &hank[0])
                return CalcDotProduct(hankPtr, fkPtr, hank.Length, shift);
        }

        private const string LibName = @"ntv_math";

        [SuppressUnmanagedCodeSecurity]
        [DllImport(LibName, EntryPoint = "CalcDotProduct")]
        private static unsafe extern Complex CalcDotProduct(double* hank, Complex* fk, int length, int shift);
    }
}
