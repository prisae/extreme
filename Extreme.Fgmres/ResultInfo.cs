﻿namespace Extreme.Fgmres
{
    public class ResultInfo
    {
        public bool ConvergenceAchieved { get; }
        public double ArnoldiBackwardError { get; }
        public double BackwardError { get; }
        public int NumberOfIterations { get; }

        public ResultInfo(bool convergenceAchieved, double arnoldiBackwardError, double backwardError, int numberOfIterations)
        {
            ConvergenceAchieved = convergenceAchieved;
            ArnoldiBackwardError = arnoldiBackwardError;
            BackwardError = backwardError;
            NumberOfIterations = numberOfIterations;
        }

        public static ResultInfo NonConverged(double arnoldiBackwardError, double backwardError, int numberOfIterations)
        {
            return new ResultInfo(
                convergenceAchieved: false,
                arnoldiBackwardError: arnoldiBackwardError,
                backwardError: backwardError,
                numberOfIterations: numberOfIterations);
        }

        public static ResultInfo Converged(double arnoldiBackwardError, double backwardError, int numberOfIterations)
        {
            return new ResultInfo(
                convergenceAchieved: true,
                arnoldiBackwardError: arnoldiBackwardError,
                backwardError: backwardError,
                numberOfIterations: numberOfIterations);
        }
    }
}
