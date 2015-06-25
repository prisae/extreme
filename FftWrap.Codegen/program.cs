using System;
using System.Collections.Generic;
using FftWrap.Codegen;

namespace FftWrap.Examples
{
    public static class Program
    {
        private static void Main(string[] args)
        {
            GenerateMpiHeaderWraperFloat();
            GenerateHeaderWraperFloat();
            
            GenerateMpiHeaderWraperDouble();
            GenerateHeaderWraperDouble();
        }


        private static void GenerateHeaderWraperFloat()
        {
            var methods = FftwHeaderParser.ParseMethods(@"..\..\..\Headers\fftw3.h");

            //PrintMethods(methods);

            CodeGenerator.DoublePrecision = false;

            CodeGenerator.GenerateCSharpCodeWithRoslyn(
                path: @"..\..\..\..\FftWrap\Fftwf.cs", 
                className:@"Fftwf", 
                dllName: @"""libfftw3f-3""", 
                methods: methods);
        }

        private static void GenerateMpiHeaderWraperFloat()
        {
            var methods = FftwHeaderParser.ParseMethodsMpi(@"..\..\..\Headers\fftw3-mpi.h");

            //PrintMethods(methods);

            CodeGenerator.DoublePrecision = false;

            CodeGenerator.GenerateMpiCSharpCodeWithRoslyn(
                path: @"..\..\..\..\FftWrap\FftwfMpi.cs", 
                className:@"FftwfMpi", 
                dllName: @"""libfftw3f-3""", 
                methods: methods);
        }

        private static void GenerateHeaderWraperDouble()
        {
            var methods = FftwHeaderParser.ParseMethods(@"..\..\..\Headers\fftw3.h");

            //PrintMethods(methods);

            CodeGenerator.DoublePrecision = true;

            CodeGenerator.GenerateCSharpCodeWithRoslyn(
                path: @"..\..\..\..\FftWrap\Fftw.cs",
                className: @"Fftw",
                dllName: @"""libfftw3-3""",
                methods: methods);
        }

        private static void GenerateMpiHeaderWraperDouble()
        {
            var methods = FftwHeaderParser.ParseMethodsMpi(@"..\..\..\Headers\fftw3-mpi.h");

            //PrintMethods(methods);

            CodeGenerator.DoublePrecision = true;

            CodeGenerator.GenerateMpiCSharpCodeWithRoslyn(
                path: @"..\..\..\..\FftWrap\FftwMpi.cs",
                className: @"FftwMpi",
                dllName: @"""libfftw3-3""",
                methods: methods);
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
