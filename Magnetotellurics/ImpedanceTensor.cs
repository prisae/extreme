using System.Numerics;

namespace Porvem.Magnetotellurics
{
    public class ImpedanceTensor : Tensor
    {
        public ImpedanceTensor(Complex xx, Complex xy, Complex yx, Complex yy)
            : base (xx, xy, yx, yy)
        {
        }
    }
}
