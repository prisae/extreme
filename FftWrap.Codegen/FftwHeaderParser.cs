using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace FftWrap.Codegen
{
    public static class FftwHeaderParser
    {
        public static IReadOnlyCollection<Method> ParseMethods(string headerPath, string namePattern)
        {
            string str = File.ReadAllText(headerPath);

            // dirty removing "noise"
            str = Regex.Replace(str, @"\\", "");
            str = Regex.Replace(str, @"\s\s+", " ");

            string pattern = @"FFTW_EXTERN\s+" +
                             @"(?<const>(const)?)\s*" +
                             @"(?<return>[(\w)]+)\s+" +
                             @"(?<pointer>\**)" +
                             namePattern +
                             @"\((?<params>[(),*\w\s]*)\)";


            var matches = Regex.Matches(str, pattern);

            var methods = new List<Method>();

            foreach (Match match in matches)
            {
                var returnType = match.Groups["return"].ToString();
                var name = match.Groups["name"].ToString();
                var parameters = match.Groups["params"];
                var isPointer = !string.IsNullOrEmpty(match.Groups["pointer"].ToString());
                var isConst = !string.IsNullOrEmpty(match.Groups["const"].Value);
                var isArray = !string.IsNullOrEmpty(match.Groups["array"].Value);
                var parametersCollection = ParseParameters(parameters.ToString());

                methods.Add(new Method(returnType, name, isPointer, isConst, parametersCollection));
            }

            return new ReadOnlyCollection<Method>(methods);
        }
        
        
        public static IReadOnlyCollection<Method> ParseMethodsMpi(string headerPath)
        {
            return ParseMethods(headerPath, @"(?<name>XM\(\w+\))\s*");
            
            //string str = File.ReadAllText(headerPath);

            //// dirty removing "noise"
            //str = Regex.Replace(str, @"\\", "");
            //str = Regex.Replace(str, @"\s\s+", " ");

            //const string pattern = @"FFTW_EXTERN\s+" +
            //                       @"(?<const>(const)+\s+" +                                   
            //                       @"(?<return>(X\(plan\))|([\w]+))\s+" +
            //                       @"(?<pointer>[*]*)" +
            //                       @"(?<name>XM\(\w+\))\s*" +
            //                       @"[(](?<params>[(),*\w\s]*)[)];";

            //var matches = Regex.Matches(str, pattern);

            //var methods = new List<Method>();

            //foreach (Match match in matches)
            //{
            //    var returnType = match.Groups["return"].ToString();
            //    var name = match.Groups["name"].ToString();
            //    var parameters = match.Groups["params"];
            //    var isPointer = !string.IsNullOrEmpty(match.Groups["pointer"].ToString());
            //    var isConst = !string.IsNullOrEmpty(match.Groups["const"].Value);

            //    var parametersCollection = ParseParameters(parameters.ToString());

            //    methods.Add(new Method(returnType, name, isPointer, isConst, parametersCollection));
            //}

            //return new ReadOnlyCollection<Method>(methods);
        }
        
        public static IReadOnlyCollection<Method> ParseMethods(string headerPath)
        {
            return ParseMethods(headerPath, @"(?<name>X[(]\w+[)])");
        }

        private static IReadOnlyCollection<Parameter> ParseParameters(string parameters)
        {
            const string pattern = @"\s*(?<const>(const)?)\s*" +                                   
                                   @"(?<type>[()\w]+)\s*" +
                                   @"(?<pointer>[*]*)" +
                                   @"(?<name>\w*)";

            var matches = Regex.Matches(parameters, pattern);

            var result = new List<Parameter>();

            foreach (Match match in matches)
            {
                var type = match.Groups["type"].ToString();
                var name = match.Groups["name"].ToString();
                var isConst = !string.IsNullOrEmpty(match.Groups["const"].Value);
                var isPointer = !string.IsNullOrEmpty(match.Groups["pointer"].ToString());

//                if (isConst)
                    //Console.WriteLine("param is const {0} {1}", name, type);

                result.Add(new Parameter(type, name, isPointer, isConst));
            }

            return new ReadOnlyCollection<Parameter>(result);
        }
    }
}
