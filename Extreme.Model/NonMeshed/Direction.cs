namespace Extreme.Model
{
    public struct Direction
    {
        public Direction(decimal start, decimal size)
        {
            Start = start;
            Size = size;
        }

        /// <summary>
        /// in meters
        /// </summary>
        public decimal Start { get; }

        /// <summary>
        /// in meters
        /// </summary>
        public decimal Size { get; }
    }
}
