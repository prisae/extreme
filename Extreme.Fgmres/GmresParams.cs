namespace Extreme.Fgmres
{
    public class GmresParams
    {
        public int BufferSize { get; }
        public int LocalDimensionSize { get; }

        public ResidualAtRestart ResidualAtRestart { get; set; } = ResidualAtRestart.ComputeTheTrue;
        public GramSchmidtType GramSchmidtType { get; set; } = GramSchmidtType.IterativeClassical;
        public InitialGuess InitialGuess { get; set; } = InitialGuess.UserSupplied;
        public Preconditioning Preconditioning { get; set; } = Preconditioning.None;
        public BackwardErrorChecking BackwardErrorChecking { get; set; }  = BackwardErrorChecking.CheckWithWarning;

        public double Tolerance { get; set; } = 1E-10;
        public int MaxRepeatNumber { get; set; } = 1;

        public GmresParams(int localDimensionSize, int bufferSize)
        {
            LocalDimensionSize = localDimensionSize;
            BufferSize = bufferSize;
        }
    }
}
