using System.Numerics;

namespace Porvem.Magnetotellurics
{
    public class ImpedanceTensor : Tensor
    {
        public static ImpedanceTensor Empty = new ImpedanceTensor(0, 0, 0, 0);

        public ImpedanceTensor(Complex xx, Complex xy, Complex yx, Complex yy)
            : base (xx, xy, yx, yy)
        {
        }
    }
}
