using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extreme.Model.Topography
{
    public class DiscretePhilippineTopographyProvider : IDiscreteTopographyProvider, IDisposable
    {
        #region Constants

        private const int FileStep = 12;

        private const int NumberOfX = 5401;
        private const int NumberOfY = 10801;

        private const double XStepAverage = 2.23466897 * 1000;
        private const double YStepAverage = 0.98050175 * 1000;

        private const double XStart = -5608.3952585 * 1000;
        private const double YStart = -5304.45072018 * 1000;

        #endregion

        private readonly BinaryReader _reader;
        private bool _preLoaded = false;
        private double[] _data;

        public static DiscretePhilippineTopographyProvider CreateProvider(string fileName)
        {
            var provider = new DiscretePhilippineTopographyProvider(fileName);
            return provider;
        }

        public static DiscretePhilippineTopographyProvider CreateProviderWithPreloadedData(string fileName)
        {
            var provider = new DiscretePhilippineTopographyProvider(fileName);
            provider.PreloadData();

            return provider;
        }

        private DiscretePhilippineTopographyProvider(string fileName)
        {
            _reader = new BinaryReader(File.Open(fileName, FileMode.Open));
        }

        public void PreloadData()
        {
            _data = new double[NumberOfX * NumberOfY * 3];

            for (int i = 0; i < _data.Length; i++)
                _data[i] = _reader.ReadDouble();

            _preLoaded = true;
        }

        public double GetMinZ()
        {
            double min = double.MaxValue;

            for (int i = 0; i < _data.Length; i += 3)
            {
                var val = _data[i + 2];

                if (val < min)
                    min = val;
            }

            return min;
        }

        public double GetMaxZ()
        {
            double max = double.MinValue;

            for (int i = 0; i < _data.Length; i += 3)
            {
                var val = _data[i + 2];

                if (val > max)
                    max = val;
            }

            return max;
        }

        public List<double> GetDepths(double x, double y, double xSize, double ySize)
        {
            int xIndexMin = (int)((x - XStart) / XStepAverage) - 1;
            int xIndexMax = (int)((x + xSize - XStart) / XStepAverage) + 1;
            int yIndexMin = (int)((y - YStart) / YStepAverage) - 1;
            int yIndexMax = (int)((y + ySize - YStart) / YStepAverage) + 1;

            var result = new List<double>();

            if (xIndexMin < 0) xIndexMin = 0;
            if (yIndexMin < 0) yIndexMin = 0;

            if (xIndexMax >= NumberOfX) xIndexMax = NumberOfX - 1;
            if (yIndexMax >= NumberOfY) yIndexMax = NumberOfY - 1;

            for (int i = xIndexMin; i <= xIndexMax; i++)
            {
                for (int j = yIndexMin; j <= yIndexMax; j++)
                {
                    var point = GetValueForIndecies(i, j);

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


        private Point GetValueForIndecies(int xIndex, int yIndex)
        {
            if (_preLoaded)
            {
                int offset = (yIndex + xIndex * NumberOfY) * 3;

                var x = -_data[offset + 0];
                var y = _data[offset + 1];
                var z = _data[offset + 2];

                return new Point(x * 1000, y * 1000, z);
            }
            else
            {
                int offset = (yIndex + xIndex * NumberOfY) * FileStep;

                _reader.BaseStream.Seek(offset, SeekOrigin.Begin);

                var x = -_reader.ReadDouble();
                var y = _reader.ReadDouble();
                var z = _reader.ReadDouble();

                return new Point(x * 1000, y * 1000, z);
            }
        }

        public void Dispose()
        {
            _reader?.Close();
        }
    }
}
