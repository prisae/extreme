using System.Numerics;

namespace Porvem.Magnetotellurics
{
    public class MagneticTipper : Tipper
    {
        public MagneticTipper(Complex zx, Complex zy)
            : base(zx, zy)
        { }

        public MagneticTipper(Tipper tipper) : base(tipper.Zx, tipper.Zy)
        {}
    }
}
