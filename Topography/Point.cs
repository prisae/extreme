using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extreme.Model.Topography
{
    public struct Point
    {
        public double X { get; private set; }
        public double Y { get; private set; }
        public Point(double x, double y)
            : this()
        {
            X = x;
            Y = y;
        }
    }
}
