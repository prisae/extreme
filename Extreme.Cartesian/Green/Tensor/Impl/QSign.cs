namespace Extreme.Cartesian.Green.Tensor
{
    public class QSigns
    {
        public readonly int XX;
        public readonly int YY;
        public readonly int ZZ;

        public readonly int XY;
        public readonly int YX;

        public readonly int XZ;
        public readonly int YZ;
        public readonly int ZX;
        public readonly int ZY;

        private QSigns(int xx, int yy, int zz, int xy, int yx, int xz, int yz, int zx, int zy)
        {
            XX = xx;
            YY = yy;
            ZZ = zz;
            XY = xy;
            YX = yx;
            XZ = xz;
            YZ = yz;
            ZX = zx;
            ZY = zy;
        }

        public static readonly QSigns AlongX = new QSigns(xx: 1, yy: -1, zz: -1, xy: -1, yx: 0, xz: 1, yz: 1, zx: 1, zy: 1);
        public static readonly QSigns AlongY = new QSigns(xx: -1, yy: 1, zz: -1, xy: -1, yx: 0, xz: 1, yz: 1, zx: 1, zy: 1);
        public static readonly QSigns AlongXSymm = new QSigns(xx: 1, yy: -1, zz: -1, xy: 1, yx: 0, xz: -1, yz: 1, zx: -1, zy: 1);
        public static readonly QSigns AlongYSymm = new QSigns(xx: -1, yy: 1, zz: -1, xy: 1, yx: 0, xz: 1, yz: -1, zx: 1, zy: -1);

        public static readonly QSigns AlongXMagnetic = new QSigns(xx: -1, yy: -1, zz: 0, xy: -1, yx: 1, xz: 1, yz: 1, zx: 1, zy: 1);
        public static readonly QSigns AlongYMagnetic = new QSigns(xx: -1, yy: -1, zz: 0, xy: 1, yx: -1, xz: 1, yz: 1, zx: 1, zy: 1);
        public static readonly QSigns AlongXSymmMagnetic = new QSigns(xx: 1, yy: 1, zz: 0, xy: -1, yx: 1, xz: 1, yz: -1, zx: 1, zy: -1);
        public static readonly QSigns AlongYSymmMagnetic = new QSigns(xx: 1, yy: 1, zz: 0, xy: 1, yx: -1, xz: 1, yz: 1, zx: -1, zy: 1);
    }
}
