//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
﻿using System;
using System.Numerics;
using Extreme.Cartesian.Core;
using Extreme.Cartesian.Green.Scalar;
using Extreme.Cartesian.Green.Tensor;
using Extreme.Cartesian.Logger;
using Extreme.Core;

namespace Extreme.Cartesian
{
    public static class DebugUtils
    {

        public static void Print(this ILogger logger, AnomalyCurrent ac, MemoryLayoutOrder layoutOrder, int k)
        {
            ac.LayoutOrder = layoutOrder;

            var la = GetLayerAccessor(ac, k);

            string result;

            for (int i = 0; i < ac.Nx; i++)
            {
                result = $"[{i:000}] : ";
                for (int j = 0; j < ac.Ny; j++)
                {
                    var val = la[i, j];
                    result += $"{Math.Sign(val.Real) * val.Magnitude:E2} ";
                }

                logger.WriteStatus(result);
            }
        }

        public static void PrintZ(AnomalyCurrent ac, MemoryLayoutOrder layoutOrder, int k)
        {
            ac.LayoutOrder = layoutOrder;

            Console.WriteLine("Z");
            Print(ac, VerticalLayerAccessor.NewZ(ac, k));
        }



        public static void PrintAll(AnomalyCurrent ac, int k)
        {
            ac.LayoutOrder = MemoryLayoutOrder.AlongVertical;


            Console.WriteLine("X");
            Print(ac, VerticalLayerAccessor.NewX(ac, k));

            Console.WriteLine("Y");
            Print(ac, VerticalLayerAccessor.NewY(ac, k));

            Console.WriteLine("Z");
            Print(ac, VerticalLayerAccessor.NewZ(ac, k));
        }

        private static void Print(AnomalyCurrent ac, ILayerAccessor la)
        {

            for (int i = 0; i < ac.Nx; i++)
            {
                Console.Write($"[{i:000}] : ");
                for (int j = 0; j < ac.Ny; j++)
                {
                    var val = la[i, j];

                    Console.Write($"{Math.Sign(val.Real) * val.Magnitude:E2} ");
                }

                Console.WriteLine();
            }
        }

        public static void PrintScalars(GreenScalars scalars, Func<SingleGreenScalar, Complex[]> select)
        {
            var ss = select(scalars.SingleScalars[0]);

            for (int i = 0; i < ss.Length; i++)
                Console.WriteLine($"{scalars.Radii[i]} {i} {ss[i]}");
    }

    public static void PrintLateral(GreenTensor gt, string comp, int ntr)
    {
        var gtc = gt[comp];

        for (int i = 0; i < gt.Nx; i++)
        {
            Console.Write($"[{i:000}] : ");
            for (int j = 0; j < gt.Ny; j++)
            {
                var val = gtc.GetAlongLateralAsym(i, j, ntr, 0);

                Console.Write($"{ val.Real:E4} ");
            }

            Console.WriteLine();
        }
    }

    public unsafe static void PrintAsym(GreenTensor gt, string comp, int rc, int tr)
    {
        var gtc = gt[comp];

        int length = gt.NTr * gt.NRc;

        for (int i = 0; i < gt.Nx; i++)
        {
            Console.Write($"[{i:000}] : ");
            for (int j = 0; j < gt.Ny; j++)
            {
                var val = gtc.Ptr[(i * gt.Ny + j) * length + rc * gt.NTr + tr];//  .GetAlongVerticalAsym(i, j, 1, 0);

                Console.Write($"{ val.Real:E4} ");
            }

            Console.WriteLine();
        }
    }

    public unsafe static void PrintSymm(GreenTensor gt, string comp, int rc, int tr)
    {
        var gtc = gt[comp];
        int nz = gt.NTr;

        int length = (nz * (nz + 1)) / 2;

        for (int i = 0; i < gt.Nx; i++)
        {
            Console.Write($"[{i:000}] : ");
            for (int j = 0; j < gt.Ny; j++)
            {
                var val = gtc.Ptr[(i * gt.Ny + j) * length + rc + tr * (tr + 1) / 2];//  .GetAlongVerticalAsym(i, j, 1, 0);

                Console.Write($"{ val.Real:E4} ");
            }

            Console.WriteLine();
        }
    }


    private static ILayerAccessor GetLayerAccessor(AnomalyCurrent ac, int zIndex)
    {
        return VerticalLayerAccessor.NewZ(ac, zIndex);
    }
}
}
