using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Extreme.Cartesian.Model;

namespace Extreme.Cartesian.Model
{
    public static class AnomalyLoaderUtils
    {
        public static void WriteAnomalyDataToPlainText(double[,,] sigma, int k, string fileName)
        {
            using (var sw = new StreamWriter(fileName))
            {
                for (int i = 0; i < sigma.GetLength(0); i++)
                {
                    for (int j = 0; j < sigma.GetLength(1); j++)
                        sw.Write("{0} ", sigma[i, j, k]);

                    sw.WriteLine();
                }
            }
        }

        public static void ReadAnomalyDataFromPlainText(LinesReader lr,  double[,,] sigma, int k)
        {
            int nx = sigma.GetLength(0);
            int ny = sigma.GetLength(1);

            var lines = lr.ReadNextLines(nx);

            // nx is from top to bottom
            // ny is from left to right

            if (lines.Length != nx)
                throw new InvalidDataException($@"model with illegal Nx value");

            for (int i = 0; i < lines.Length; i++)
            {
                var lineValues = GetAllDouble(lines[i]);

                if (lineValues.Length != ny)
                    throw new InvalidDataException(
                        string.Format($"model with illegal Ny value in line {i}"));

                for (int j = 0; j < lineValues.Length; j++)
                    sigma[i, j, k] = lineValues[j];
            }
        }

        private static double[] GetAllDouble(string str)
        {
            return GetAllDouble(str, ' ', '\t');
        }

        private static double[] GetAllDouble(this string str, params char[] seperators)
        {
            var values = str.Split(seperators).Where(s => !string.IsNullOrEmpty((s))).Select(v => v.Replace('D', 'E'));

            return values.Select(v => double.Parse(v, NumberStyles.Float, CultureInfo.InvariantCulture)).ToArray();
        }

        public static void ParseApplique(string template, double[,,] sigma, int k)
        {
            var splitted = template
                .Split('\n')
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrEmpty(s)).ToArray();

            int prevLines = 0;

            for (int i = 0; i < splitted.Length; i++)
            {
                int lines = GetLines(splitted[i]);

                List<Tuple<int, float>> values = GetColumnsAndValues(splitted[i]);

                FillSigma(sigma, k, prevLines, lines, values);

                prevLines += lines;
            }
        }

        private static void FillSigma(double[,,] sigma, int k, int prevLines, int lines, List<Tuple<int, float>> values)
        {
            CheckSizes(sigma, prevLines, lines, values);

            for (int i = prevLines; i < prevLines + lines; i++)
            {
                int prevColums = 0;

                for (int index = 0; index < values.Count; index++)
                {
                    var columns = values[index].Item1;
                    var value = values[index].Item2;

                    for (int j = prevColums; j < prevColums + columns; j++)
                        sigma[j, i, k] = value;

                    prevColums += columns;
                }
            }
        }

        private static void CheckSizes(double[,,] sigma, int prevLines, int lines, List<Tuple<int, float>> values)
        {
            int nx = sigma.GetLength(1);
            int ny = sigma.GetLength(0);

            if (prevLines > nx || prevLines + lines > nx)
                throw new CartesianModelLoadException("Applique nx is out of range");

            for (int i = prevLines; i < prevLines + lines; i++)
            {
                int prevColums = 0;

                for (int index = 0; index < values.Count; index++)
                {
                    var columns = values[index].Item1;

                    if (prevColums > ny || prevColums + columns > ny)
                        throw new CartesianModelLoadException("Applique ny is out of range");

                    prevColums += columns;
                }
            }
        }

        private static List<Tuple<int, float>> GetColumnsAndValues(string str)
        {
            const string template = @"\s*(?<columns>\d+)\*(?<value>[0-9]*\.?[0-9]+([eE][-+]?[0-9]+)*)";
            var rgx = new Regex(template, RegexOptions.IgnoreCase);

            var result = new List<Tuple<int, float>>();

            var matches = rgx.Matches(str);

            foreach (Match match in matches)
            {
                int columns = int.Parse(match.Groups["columns"].Value);
                float value = float.Parse(match.Groups["value"].Value, NumberStyles.Float, CultureInfo.InvariantCulture);

                result.Add(new Tuple<int, float>(columns, value));
            }

            return result;
        }

        private static int GetLines(string str)
        {
            const string linesPattern = @"(?<lines>\d+)LINES:";
            var rgx = new Regex(linesPattern, RegexOptions.IgnoreCase);
            var match = rgx.Match(str);
            return int.Parse(match.Groups["lines"].Value);
        }
    }
}
