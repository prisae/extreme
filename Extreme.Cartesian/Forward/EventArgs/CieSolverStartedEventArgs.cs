﻿using System;
using Extreme.Cartesian.Core;

namespace Extreme.Cartesian.Forward
{
    public class CieSolverStartedEventArgs : EventArgs
    {
        public AnomalyCurrent Rhs { get; }
        public AnomalyCurrent InitialGuess { get; }

        public CieSolverStartedEventArgs(AnomalyCurrent rhs, AnomalyCurrent initialGuess)
        {
            Rhs = rhs;
            InitialGuess = initialGuess;
        }
    }
}
