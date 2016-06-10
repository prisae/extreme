using System.Numerics;

namespace Porvem.Magnetotellurics
{
    public class QuasiElectricTensor : Tensor
    {
        public QuasiElectricTensor(Complex xx, Complex xy, Complex yx, Complex yy)
            : base (xx, xy, yx, yy)
        {
        }
    }
}
