//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
﻿using System;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Extreme.Model.Topography
{
    public class UniformGridTopographyLoader
    {
        private readonly string _fileName;
        private readonly Func<Point, Point> _coordinateConverter;

        public UniformGridTopographyLoader(string fileName)
        {
            _fileName = fileName;
            _coordinateConverter = EmptyConverter;
        }

        public UniformGridTopographyLoader(string fileName, Func<Point, Point> coordinateConverter)
        {
            if (fileName == null) throw new ArgumentNullException(nameof(fileName));
            if (coordinateConverter == null) throw new ArgumentNullException(nameof(coordinateConverter));

            _fileName = fileName;
            _coordinateConverter = coordinateConverter;
        }

        private static Point EmptyConverter(Point p) => p;

        public UniformGridTopographyProvider LoadFromBinFile(int nx, int ny, double dx, double dy)
        {
            using (var br = new BinaryReader(File.OpenRead(_fileName)))
                return LoadFromBinFile(br, nx, ny, dx, dy);
        }

        private UniformGridTopographyProvider LoadFromBinFile(BinaryReader br, int nx, int ny, double dx, double dy)
        {
            var points = new Point[nx, ny];

            for (int i = 0; i < nx; i++)
                for (int j = 0; j < ny; j++)
                {
                    var p = new Point(br.ReadDouble(), br.ReadDouble(), br.ReadDouble());
                    p = _coordinateConverter(p);
                    points[i, j] = p;
                }

            return new UniformGridTopographyProvider(points, nx, ny, dx, dy);
        }

        public UniformGridTopographyProvider LoadFromXyzFile(int nx, int ny, float dx, float dy)
        {
            using (var sr = new StreamReader(_fileName))
                return LoadFromXyzFile(sr, nx, ny, dx, dy);
        }

        private UniformGridTopographyProvider LoadFromXyzFile(StreamReader sr, int nx, int ny, float dx, float dy)
        {
            var points = new Point[nx, ny];

            for (int i = 0; i < nx; i++)
            {
                for (int j = 0; j < ny; j++)
                {
                    var values = ReadLine(sr);

                    var p = new Point(values[0], values[1], values[2]);
                    p = _coordinateConverter(p);
                    points[i, j] = p;
                }
            }

            return new UniformGridTopographyProvider(points, nx, ny, dx, dy);
        }

        private static float[] ReadLine(StreamReader sr)
        {
            var line = sr.ReadLine();
            var strs = line.Split(' ').Where(s => !string.IsNullOrEmpty(s)).ToArray();

            var values = new float[strs.Length];

            for (int i = 0; i < values.Length; i++)
                values[i] = float.Parse(strs[i], NumberStyles.Float, CultureInfo.InvariantCulture);

            return values;
        }
    }
}
