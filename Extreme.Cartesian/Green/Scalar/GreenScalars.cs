namespace Extreme.Cartesian.Green.Scalar
{
    public class GreenScalars
    {
        public readonly double[] Radii;
        public readonly SingleGreenScalar[] SingleScalars;
        public GreenScalars(double[] radii, SingleGreenScalar[] scalars)
        {
            Radii = radii;
            SingleScalars = scalars;
        }

        public int GetNumberOfAvailableIs(ScalarPlan plan)
        {
            int counter = 0;

            if (plan.CalculateI1) counter++;
            if (plan.CalculateI2) counter++;
            if (plan.CalculateI3) counter++;
            if (plan.CalculateI4) counter++;
            if (plan.CalculateI5) counter++;

            return counter;
        }
    }
}
