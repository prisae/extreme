using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extreme.Core.Model
{
    public class Layer1D
    {
        protected Layer1D(decimal thicknessInMeters)
        {
            Thickness = thicknessInMeters;
        }

        /// <summary>
        /// Thickness of layer in meters
        /// </summary>
        public decimal Thickness { get; private set; }
    }
}
