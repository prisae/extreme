using System.Numerics;

namespace Porvem.Magnetotellurics
{
    public class MagneticTipper : Tipper
    {
        public static MagneticTipper Empty = new MagneticTipper(0, 0);

        public MagneticTipper(Complex zx, Complex zy)
            : base(zx, zy)
        { }

        public MagneticTipper(Tipper tipper) : base(tipper.Zx, tipper.Zy)
        {}
    }
}
