//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
﻿using System;

namespace Extreme.Parallel
{
    public class ParallelTask
    {
        private readonly double _period;
        private readonly double _frequency;
        private readonly int _polarizationIndex;

        public static ParallelTask NewFrequencyTask(double frequency, int polarizationIndex)
        {
            if (frequency <= 0) throw new ArgumentOutOfRangeException(nameof(frequency));

            return new ParallelTask(frequency, -1, polarizationIndex);
        }

     
        private ParallelTask(double frequency, double period, int polarizationIndex)
        {
            _frequency = frequency;
            _period = period;
            _polarizationIndex = polarizationIndex;
        }

        public double Frequency
        {
            get { return _frequency; }
        }
        public double Period
        {
            get { return _period; }
        }

        public int PolarizationIndex
        {
            get { return _polarizationIndex; }
        }

        public bool IsPeriod
        {
            get { return _period > 0; }
        }

        public bool IsFrequency
        {
            get { return _frequency > 0; }
        }
    }
}
