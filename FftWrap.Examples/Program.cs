using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Xml.Serialization;
using FftWrap.Codegen;
using FftWrap.Numerics;
using Porvem.Parallel;

namespace FftWrap.Examples
{
    public static class Program
    {
        private static void Main(string[] args)
        {
            //Perform2DMpiInterleaved();
            //Perform2DMpi();

            //Perform2DMpiComparison();


            Perform1DTransformDirect();
            //Perform1DTransform();
        }

        private static void Perform2DMpiComparison()
        {
            using (var mpi = new Mpi())
            {
                FftwMpi.Init();

                try
                {
                    const int size1 = 800;
                    const int size2 = 1600;
                    const int alongZ = 500;

                    Perform2DMpiInterleavedHighLoaded(mpi, size1, size2, alongZ);
                    mpi.Barrier();

                    Perform2DMpiHighLoaded(mpi, size1, size2, alongZ);
                    mpi.Barrier();


                }

                finally
                {
                    FftwMpi.Cleanup();
                }
            }
        }

        private static void Perform2DMpiHighLoaded(Mpi mpi, int size1, int size2, int alongZ)
        {
            if (mpi.IsMaster)
            {
                Console.WriteLine("Run {0} {1} {2} SEPARATED", size1, size2, alongZ);
                Console.WriteLine("Prepare data...");
            }

            var plans = new List<DistributedPlan>(alongZ);
            
            for (int i = 0; i < alongZ; i++)
            {
                var plan = DistributedPlan.CreateNewPlan2D(Mpi.CommWorld, size1, size2);
                plan.SetAllValuesTo(SingleComplex.Zero);
                plan.SetValue(0, 0, SingleComplex.One);

                plans.Add(plan);
            }
            
            var start = DateTime.Now;

            if (mpi.IsMaster)
                Console.WriteLine("Run forward and backward...");

            foreach (var plan in plans)
            {
                plan.RunForward();
                plan.RunBackward();

            }
            
            var end = DateTime.Now;

            var val = plans[alongZ - 1].GetValue(0, 0);
            if (val != null)
                Console.WriteLine("Result: {0}", val);

            if (mpi.IsMaster)
                Console.WriteLine("time: {0}", end - start);


            plans.ForEach(p => p.Dispose());
        }

        private static void Perform2DMpiInterleavedHighLoaded(Mpi mpi, int size1, int size2, int alongZ)
        {
            using (var plan = DistributedPlan.CreateNewPlan2D(Mpi.CommWorld, size1, size2, alongZ))
            {
                if (mpi.IsMaster)
                {
                    Console.WriteLine("Run {0} {1} {2} INTERLEAVED", size1, size2, alongZ);
                    Console.WriteLine("Prepare data...");
                }

                plan.SetAllValuesTo(SingleComplex.Zero);

                for (int i = 0; i < alongZ; i++)
                    plan.SetValue(0, 0, i, SingleComplex.One);

                var start = DateTime.Now;

                if (mpi.IsMaster)
                    Console.WriteLine("Run forward and backward...");

                plan.RunForward();
                plan.RunBackward();

                var end = DateTime.Now;

                var val = plan.GetValue(0, 0, alongZ-1);
                if (val != null)
                    Console.WriteLine("Result: {0}", val);

                if (mpi.IsMaster)
                    Console.WriteLine("time: {0}", end - start);

            }
        }

        private static void Perform2DMpi()
        {
            using (var mpi = new Mpi())
            {
                Console.WriteLine("rank {0} of {1}", mpi.Rank, mpi.Size);

                int size1 = 4;
                int size2 = 3;

                FftwMpi.Init();

                try
                {
                    using (var plan = DistributedPlan.CreateNewPlan2D(Mpi.CommWorld, size1, size2))
                    {
                        plan.SetAllValuesTo(SingleComplex.Zero);
                        plan.SetValue(0, 0, SingleComplex.One);

                        Console.WriteLine("r:{0} {1} {2}", mpi.Rank, plan.LocalSize1Start, plan.LocalSize1);

                        PrintAllValues(mpi, plan);

                        plan.RunForward();
                        plan.RunBackward();

                        Console.WriteLine();
                        PrintAllValues(mpi, plan);
                    }

                    mpi.Barrier();
                }

                finally
                {
                    FftwMpi.Cleanup();
                }
            }
        }

        private static void Perform2DMpiInterleaved()
        {
            using (var mpi = new Mpi())
            {
                Console.WriteLine("rank {0} of {1}", mpi.Rank, mpi.Size);

                const int alongZ = 3;

                const int size1 = 4;
                const int size2 = 3;

                FftwMpi.Init();

                try
                {
                    using (var plan = DistributedPlan.CreateNewPlan2D(Mpi.CommWorld, size1, size2, alongZ))
                    {
                        plan.SetAllValuesTo(SingleComplex.Zero);
                        plan.SetValue(0, 0, 0, SingleComplex.One);
                        plan.SetValue(0, 0, 1, SingleComplex.One);
                        plan.SetValue(0, 0, 2, SingleComplex.One);

                        Console.WriteLine("r:{0} {1} {2}", mpi.Rank, plan.LocalSize1Start, plan.LocalSize1);

                        PrintAllValues(mpi, plan);

                        plan.RunForward();
                        plan.RunBackward();

                        Console.WriteLine();
                        PrintAllValues(mpi, plan);
                    }

                    mpi.Barrier();
                }

                finally
                {
                    FftwMpi.Cleanup();
                }
            }
        }



        private static void PrintAllValues(Mpi mpi, DistributedPlan plan)
        {
            if (plan.Interleaved == 1)
                PrintAllValuesNotInterleaved(mpi, plan);
            else
                PrintAllValuesInterleaved(mpi, plan);
        }

        private static void PrintAllValuesInterleaved(Mpi mpi, DistributedPlan plan)
        {
            mpi.Barrier();

            for (int k = 0; k < plan.Interleaved; k++)
            {
                for (int i = 0; i < plan.FullSize1; i++)
                    for (int j = 0; j < plan.FullSize2; j++)
                    {
                        var value = plan.GetValue(i, j, k);

                        if (value.HasValue)
                            Console.WriteLine("r:{0}, k:{4} m[{1},{2}]={3}", mpi.Rank, i, j, value.Value, k);
                    }

                mpi.Barrier();

                if (mpi.IsMaster)
                    Console.WriteLine();
            }
        }

        private static void PrintAllValuesNotInterleaved(Mpi mpi, DistributedPlan plan)
        {
            for (int i = 0; i < plan.FullSize1; i++)
                for (int j = 0; j < plan.FullSize2; j++)
                {
                    var value = plan.GetValue(i, j);

                    if (value.HasValue)
                        Console.WriteLine("r:{0} m[{1},{2}]={3}", mpi.Rank, i, j, value.Value);
                }
        }

        private static void Perform2DMpiDirect()
        {
            using (var mpi = new Mpi())
            {
                Console.WriteLine("rank {0} of {1}", mpi.Rank, mpi.Size);


                var size1 = (IntPtr)4;
                var size2 = (IntPtr)1;

                IntPtr ptrLocalN0;
                IntPtr ptrLocalN0Start;

                FftwMpi.Init();



                IntPtr localSize = FftwMpi.LocalSizeMany(2, new IntPtr[] { size1, size2 }, new IntPtr(1), new IntPtr(0), Mpi.CommWorld, out ptrLocalN0, out ptrLocalN0Start);
                //IntPtr localSize = FftwMpi.LocalSize2D(size1, size2, Mpi.CommWorld, out ptrLocalN0, out ptrLocalN0Start);


                IntPtr srcPtr = Fftw.AllocComplex(localSize);

                var matrix = new NativeMatrix<SingleComplex>(srcPtr, (int)ptrLocalN0, (int)size2);

                ClearArray(mpi, (int)ptrLocalN0Start, (int)ptrLocalN0, (int)size2, matrix);

                if (mpi.Rank == 0)
                    matrix[0, 0] = SingleComplex.One;

                PrintArray(mpi, (int)ptrLocalN0Start, (int)ptrLocalN0, (int)size2, matrix);


                var plan1 = FftwMpi.PlanDft2D(size1, size2, srcPtr, srcPtr, Mpi.CommWorld, (int)Direction.Forward, (uint)Flags.Estimate);
                var plan2 = FftwMpi.PlanDft2D(size1, size2, srcPtr, srcPtr, Mpi.CommWorld, (int)Direction.Backward, (uint)Flags.Estimate);



                Fftw.Execute(plan1);
                Fftw.Execute(plan2);


                Console.WriteLine();

                PrintArray(mpi, (int)ptrLocalN0Start, (int)ptrLocalN0, (int)size2, matrix);

                Fftw.DestroyPlan(plan1);
                Fftw.DestroyPlan(plan2);

                Fftw.Free(srcPtr);


                FftwMpi.Cleanup();
            }
        }


        private static void PrintArray(Mpi mpi, int n0Start, int n0Size, int size2, NativeMatrix<SingleComplex> matrix)
        {
            for (int i = 0; i < n0Size; i++)
                for (int j = 0; j < size2; j++)
                    Console.WriteLine("r:{0} m[{1},{2}]={3}", mpi.Rank, i + n0Start, j, matrix[i, j]);
        }


        private static void ClearArray(Mpi mpi, int n0Start, int n0Size, int size2, NativeMatrix<SingleComplex> matrix)
        {
            for (int i = 0; i < n0Size; i++)
                for (int j = 0; j < size2; j++)
                    matrix[i, j] = SingleComplex.Zero;
        }


        /// <summary>
        /// Example using high-level wrapping
        /// </summary>
        public static void Perform1DTransform()
        {
            var arr1 = Memory.AllocateArray<SingleComplex>(4);
            var arr2 = Memory.AllocateArray<SingleComplex>(4);

            try
            {
                arr1.SetEach(SingleComplex.Zero);
                arr2.SetEach(SingleComplex.Zero);

                arr1[0] = SingleComplex.One;
                arr2[0] = SingleComplex.ImaginaryOne;

                using (var plan1 = Plan.Create(arr1, Direction.Backward))
                using (var plan2 = Plan.Create(arr1, Direction.Backward))
                {
                    plan1.Execute();
                    plan2.Execute();

                    plan1.Execute(arr2);
                    plan2.Execute(arr2);
                }

                arr1.ForEach(c => Console.WriteLine(c));
                Console.WriteLine();
                arr2.ForEach(c => Console.WriteLine(c));
            }

            finally
            {
                Memory.FreeAllPointers();
            }
        }

        /// <summary>
        /// Using fftw directly without high-level wrapper
        /// </summary>
        public static void Perform1DTransformDirect()
        {
            int length = 100;

            
            IntPtr srcPtr = Fftw.AllocComplex((IntPtr)length);
            IntPtr dstPtr = Fftw.AllocComplex((IntPtr)length);

            try
            {
                var src = new NativeArray<Complex>(srcPtr, length);
                var dst = new NativeArray<Complex>(dstPtr, length);

                src.SetEach(Complex.Zero);
                src[0] = Complex.One;

                IntPtr plan1 = Fftw.PlanDft1D(length, srcPtr, dstPtr, (int)Direction.Forward, (uint)Flags.Estimate);
                IntPtr plan2 = Fftw.PlanDft1D(length, dstPtr, srcPtr, (int)Direction.Backward, (uint)Flags.Estimate);

                Fftw.Execute(plan1);
                Fftw.Execute(plan2);

                Fftw.DestroyPlan(plan1);
                Fftw.DestroyPlan(plan2);

                Console.WriteLine(src[0]);
            }

            finally
            {
                Fftw.Free(srcPtr);
                Fftw.Free(dstPtr);
            }
        }


        private static void PrintMethods(IReadOnlyCollection<Method> methods)
        {
            foreach (var method in methods)
            {
                var name = method.NameToCSharp();
                var type = method.TypeNameToCSharp();

                Console.WriteLine("\n{0} {1}", type, name);

                foreach (var parameter in method.Parameters)
                {
                    var ptype = parameter.TypeNameToCSharp();
                    Console.WriteLine("\t{0} {1}", ptype, parameter.Name);
                }
            }

            Console.WriteLine("Total {0} methods", methods.Count);
        }
    }
}
