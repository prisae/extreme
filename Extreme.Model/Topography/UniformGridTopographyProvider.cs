using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Extreme.Model.Topography
{
    public class UniformGridTopographyProvider : IDiscreteTopographyProvider
    {
        public double StartX { get; }
        public double StartY { get; }

        public int Nx { get; }
        public int Ny { get; }

        public double AverageSizeX { get; }
        public double AverageSizeY { get; }

        private readonly Point[,] _points;

        public UniformGridTopographyProvider(Point[,] points, int nx, int ny, double averageSizeX, double averageSizeY)
        {
            if (points == null) throw new ArgumentNullException(nameof(points));
            _points = points;

            Nx = nx;
            Ny = ny;
            AverageSizeX = averageSizeX;
            AverageSizeY = averageSizeY;

            StartX = _points[0, 0].X;
            StartY = _points[0, 0].Y;
        }

        public double GetMinZ()
        {
            double min = double.MaxValue;

            for (int i = 0; i < Nx; i++)
            {
                for (int j = 0; j < Ny; j++)
                {
                    var val = _points[i, j].Z;

                    if (val < min)
                        min = val;
                }
            }

            return min;
        }

        public double GetMaxZ()
        {
            double max = double.MinValue;

            for (int i = 0; i < Nx; i++)
            {
                for (int j = 0; j < Ny; j++)
                {
                    var val = _points[i, j].Z;
                    if (val > max)
                        max = val;
                }
            }

            return max;
        }

        public List<double> GetDepths(double x, double y, double xSize, double ySize)
        {
            int xIndexMin = (int)((x - StartX) / AverageSizeX) - 1;
            int xIndexMax = (int)((x + xSize - StartX) / AverageSizeX) + 1;
            int yIndexMin = (int)((y - StartY) / AverageSizeY) - 1;
            int yIndexMax = (int)((y + ySize - StartY) / AverageSizeY) + 1;

            var result = new List<double>();

            if (xIndexMin < 0) xIndexMin = 0;
            if (yIndexMin < 0) yIndexMin = 0;

            if (xIndexMax >= Nx) xIndexMax = Nx - 1;
            if (yIndexMax >= Ny) yIndexMax = Ny - 1;

            for (int i = xIndexMin; i <= xIndexMax; i++)
            {
                for (int j = yIndexMin; j <= yIndexMax; j++)
                {
                    var point = _points[i, j];

                    // double-check
                    if (Contains(x, y, xSize, ySize, point.X, point.Y))
                        result.Add(point.Z);
                }
            }

            return result;
        }

        private bool Contains(double thisX, double thisY, double thisWidth, double thisHeight, double x, double y)
        {
            return thisX <= x &&
                   thisY <= y &&
                   x < thisX + thisWidth &&
                   y < thisY + thisHeight;
        }
    }
}
