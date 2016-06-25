//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using Extreme.Cartesian.Model;
using Extreme.Core;

namespace Extreme.Cartesian.Magnetotellurics
{
    public class ResultsContainer : IDisposable
    {
        public List<AllFieldsAtLevel> LevelFields { get; }
        public LateralDimensions Lateral { get; }
		public int LocalNx;
		public int LocalNxStart;
        private bool _isDisposed = false;

		public ResultsContainer(LateralDimensions lateral,int nx=-1, int nx_offset=0)
        {
            Lateral = lateral;
            LevelFields = new List<AllFieldsAtLevel>();
			LocalNx = nx < 0 ? lateral.Nx : nx;
			LocalNxStart = nx_offset;
        }

        public void Add(AllFieldsAtLevel levelField)
        {
            LevelFields.Add(levelField);
        }

        public void Dispose()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(this.GetType().ToString());

            foreach (var atLevel in LevelFields)
            {
                atLevel.NormalE1.Dispose();
                atLevel.NormalE2.Dispose();
                atLevel.NormalH1.Dispose();
                atLevel.NormalH2.Dispose();

                atLevel.AnomalyE1.Dispose();
                atLevel.AnomalyE2.Dispose();
                atLevel.AnomalyH1.Dispose();
                atLevel.AnomalyH2.Dispose();
            }

            _isDisposed = true;
        }

        //     public void SaveToFile(string fileName) => SaveToXDocument().Save(fileName);

        public static ResultsContainer Load(string fileName, LateralDimensions lateral = default(LateralDimensions))
            => Load(XDocument.Load(fileName), lateral);

        public static ResultsContainer Load(XDocument xdoc, LateralDimensions lateral = default(LateralDimensions))
        {
            var xresult = xdoc.Element("ResultsMT");

            if (xresult == null)
                throw new ArgumentOutOfRangeException("xdoc");

            if (lateral == default(LateralDimensions))
                lateral = ModelReader.LateralDimensionsFromXElement(xresult);

            if (lateral == default(LateralDimensions))
                throw new InvalidOperationException("no lateral dimensions");

            var xfreq = xresult.Element("Frequencies");
            var xobs = xresult.Element("Observations");
            var xvals = xresult.Element("Values");

            if (xfreq == null) throw new InvalidDataException("Frequencies");
            if (xobs == null) throw new InvalidDataException("Observations");
            if (xvals == null) throw new InvalidDataException("Values");

            var result = new ResultsContainer(lateral);

            return result;
        }


    }
}
