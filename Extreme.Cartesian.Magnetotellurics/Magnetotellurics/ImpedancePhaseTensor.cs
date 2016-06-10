using System.Numerics;

namespace Porvem.Magnetotellurics
{
    public class ImpedancePhaseTensor : PhaseTensor
    {
        public ImpedancePhaseTensor(double xx, double xy, double yx, double yy)
            : base(xx, xy, yx, yy)
        {
        }
    }
}
