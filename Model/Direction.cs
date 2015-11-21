using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extreme.Core.Model
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
