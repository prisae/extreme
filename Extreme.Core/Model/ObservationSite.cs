//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
﻿namespace Extreme.Core
{
    public class ObservationSite
    {
        private readonly decimal _x;
        private readonly decimal _y;
        private readonly decimal _z;
        private readonly string _name;

        public ObservationSite(decimal x, decimal y, decimal z):
            this (x, y, z, string.Empty)
        {
        }

        public ObservationSite(decimal x, decimal y, decimal z, string name)
        {
            _x = x;
            _y = y;
            _z = z;
            _name = name;
        }

        public string Name
        {
            get { return _name; }
        }

        public decimal X
        {
            get { return _x; }
        }

        public decimal Y
        {
            get { return _y; }
        }

        public decimal Z
        {
            get { return _z; }
        }
    }
}
