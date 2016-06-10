using System.Numerics;

namespace Porvem.Magnetotellurics
{
    public class PhaseTensor
    {
     private readonly double _xx;
        private readonly double  _xy;
        private readonly double  _yx;
        private readonly double  _yy;

        public PhaseTensor(double xx, double xy, double yx, double yy)
        {
            _xx = xx;
            _xy = xy;
            _yx = yx;
            _yy = yy;
        }

        public double  Xx
        {
            get { return _xx; }
        }

        public double  Xy
        {
            get { return _xy; }
        }

        public double  Yx
        {
            get { return _yx; }
        }

        public double  Yy
        {
            get { return _yy; }
        }
    }
}
