namespace Extreme.Core
{
    public class LateralDimensions
    {
        public LateralDimensions(int nx, int ny, decimal cellSizeX, decimal cellSizeY)
        {
            Nx = nx;
            Ny = ny;
            CellSizeX = cellSizeX;
            CellSizeY = cellSizeY;
        }

        public int Nx { get; }
        public int Ny { get; }
        public decimal CellSizeX { get; }
        public decimal CellSizeY { get; }
    }
}
