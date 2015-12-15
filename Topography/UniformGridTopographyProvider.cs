using System.Globalization;
using System.IO;
using System.Linq;

namespace Extreme.Model.Topography
{
    public class UniformGridTopographyProvider
    {
        public float X0 { get; }
        public float Y0 { get; }

        public int Nx { get; }
        public int Ny { get; }

        public float Dx { get;}
        public float Dy { get; }

        private readonly float[,] _depths;

        public UniformGridTopographyProvider(float[,] depths, float x0, float y0, int nx, int ny, float dx, float dy)
        {
            _depths = depths;
            X0 = x0;
            Y0 = y0;
            Nx = nx;
            Ny = ny;
            Dx = dx;
            Dy = dy;
        }

        public static UniformGridTopographyProvider LoadFromXyzFile(string fileName, int nx, int ny, float dx, float dy)
        {
            using (var sr = new StreamReader(fileName))
                return LoadFromXyzFile(sr, nx, ny, dx, dy);
        }

        private static UniformGridTopographyProvider LoadFromXyzFile(StreamReader sr, int nx, int ny, float dx, float dy)
        {
            var depths  = new float[nx, ny];

            var first = true;

            var x0 = 0f;
            var y0 = 0f; 

            for (int i = 0; i < nx; i++)
            {
                for (int j = 0; j < ny; j++)
                {
                    var values = ReadLine(sr);

                    if (first)
                    {
                        x0 = values[0];
                        y0 = values[1];
                    }
                    
                    depths[i, j] = values[2];

                    first = false;
                }
            }

            return new UniformGridTopographyProvider(depths, x0, y0, nx, ny, dx, dy);
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
