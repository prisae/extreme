using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Extreme.Core;

namespace Extreme.Cartesian.Green.Tensor
{
    public class TensorPlan
    {
        public int NxTotalLength { get; }
        public int NxCalcLength { get; }
        public int NxStart { get; }
        public int NRc { get; }
        public int NTr { get; }


        public decimal ShiftX { get; } = 0;
        public decimal ShiftY { get; } = 0;

        public TensorPlan(int nxStart, int nxCalcLength, int nxTotalLength, int nTr, int nRc)
        {
            NRc = nRc;
            NTr = nTr;
            NxStart = nxStart;
            NxCalcLength = nxCalcLength;
            NxTotalLength = nxTotalLength;
        }

        public TensorPlan(TensorPlan plan, SourceLayer src, ObservationLevel level)
        {
            NxStart = plan.NxStart;
            NxCalcLength = plan.NxCalcLength;
            NxTotalLength = plan.NxTotalLength;
            NRc = plan.NRc;
            NTr = plan.NTr;

            ShiftX = level.ShiftAlongX - src.ShiftAlongX;
            ShiftY = level.ShiftAlongY - src.ShiftAlongY;
        }

        public TensorPlan(TensorPlan plan, ObservationLevel level)
        {
            NxStart = plan.NxStart;
            NxCalcLength = plan.NxCalcLength;
            NxTotalLength = plan.NxTotalLength;
            NRc = plan.NRc;
            NTr = plan.NTr;

            ShiftX = level.ShiftAlongX;
            ShiftY = level.ShiftAlongY;
        }

        public TensorPlan(TensorPlan plan, SourceLayer layer)
        {
            NxStart = plan.NxStart;
            NxCalcLength = plan.NxCalcLength;
            NxTotalLength = plan.NxTotalLength;
            NRc = plan.NRc;
            NTr = plan.NTr;

            ShiftX = layer.ShiftAlongX;
            ShiftY = layer.ShiftAlongY;
        }
    }
}
