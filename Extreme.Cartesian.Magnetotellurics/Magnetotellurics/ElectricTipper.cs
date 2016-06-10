using System.Numerics;

namespace Porvem.Magnetotellurics
{
    public class ElectricTipper : Tipper
    {
        public ElectricTipper(Complex zx, Complex zy)
            : base(zx, zy)
        { }

        public ElectricTipper(Tipper tipper) : base(tipper.Zx, tipper.Zy)
        {
        }
    }
}
