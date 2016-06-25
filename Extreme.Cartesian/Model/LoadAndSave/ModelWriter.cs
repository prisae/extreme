//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Extreme.Core;

namespace Extreme.Cartesian.Model
{
    public class ModelWriter : ModelSerializer
    {
        public static void Save(string path, CartesianModel model)
        {
            var xdoc = new XDocument();

            var xmodel = ToXElement(model, null);

            xdoc.Add(xmodel);
            xdoc.Save(path);
        }

        public static void SaveWithPlainTextAnomaly(string path, CartesianModel model, string[] fileNames)
        {
            if (fileNames.Length != model.Anomaly.Layers.Count)
                throw new ArgumentOutOfRangeException(nameof(fileNames));

            var xdoc = new XDocument();

            var xmodel = ToXElement(model, fileNames);

            xdoc.Add(xmodel);
            xdoc.Save(path);
        }

        public static void SaveWithPlainTextAnomaly(string path, CartesianModel model)
        {
            var dir = Path.GetDirectoryName(path);
            var fileNames = SaveAnomalyLayersToPlainText(dir, model.Anomaly);
            SaveWithPlainTextAnomaly(path, model, fileNames);
        }


        public static XElement ToXElement(CartesianModel model)
        {
            return ToXElement(model, null);
        }

        private static string[] SaveAnomalyLayersToPlainText(string path, CartesianAnomaly cartesianAnomaly)
        {
            var fileNames = new string[cartesianAnomaly.Layers.Count];

            for (int k = 0; k < fileNames.Length; k++)
            {
                fileNames[k] = $"anomaly_layer_{k:0000}.dat";
                AnomalyLoaderUtils.WriteAnomalyDataToPlainText(cartesianAnomaly.Sigma, k, Path.Combine(path, fileNames[k]));
            }

            return fileNames;
        }


        private static XElement ToXElement(CartesianModel model, string[] fileNames)
        {
            var xmodel = new XElement(Model, new XAttribute(ModelVersionAttr, SecondVersion));

            SaveLateralDimensions(xmodel, model.LateralDimensions);
            SaveBackground(xmodel, model.Section1D);
            SaveAnomalies(xmodel, model.Anomaly, fileNames);

            return xmodel;
        }

        private static void SaveLateralDimensions(XElement xmodel, LateralDimensions lateral)
        {
            xmodel.Add(ToXElement(lateral));
        }

        public static XElement ToXElement(LateralDimensions lateralDimensions)
        {
            return new XElement(LateralDimensionsItem,
                new XAttribute(LateralDimensionsNxAttr, lateralDimensions.Nx),
                new XAttribute(LateralDimensionsNyAttr, lateralDimensions.Ny),
                new XAttribute(LateralDimensionsDxAttr, lateralDimensions.CellSizeX),
                new XAttribute(LateralDimensionsDyAttr, lateralDimensions.CellSizeY));
        }

        public static void SaveBackground(XElement xmodel, CartesianSection1D section1D)
        {
            var xbackground = new XElement(BackgroundSection,
                                    new XAttribute(BackgroundZeroLevelAttr, section1D.ZeroAirLevelAlongZ));

            for (int i = 0; i < section1D.NumberOfLayers; i++)
            {
                xbackground.Add(new XElement(BackgroundLayer,
                                    new XAttribute(BackgroundLayerThicknessAttr, section1D[i].Thickness),
                                    new XAttribute(BackgroundLayerSigmaRealAttr, section1D[i].Sigma.ToString("E2"))));
            }

            xmodel.Add(xbackground);
        }

        private static void SaveAnomalies(XElement xmodel, CartesianAnomaly cartesianAnomaly, string[] fileNames)
        {
            var layers = cartesianAnomaly.Layers;

            var xlayers = new List<XElement>();

            if (fileNames != null)
                for (int i = 0; i < layers.Count; i++)
                    xlayers.Add(ToXElement(layers[i], fileNames[i]));
            else
                xlayers.AddRange(layers.Select(l => ToXElement(l, null)));

            xmodel.Add(new XElement(AnomalySection, xlayers));
        }

        private static XElement ToXElement(CartesianAnomalyLayer layer, string fileName)
        {
            var xelem = new XElement(AnomalyLayer,
                           new XAttribute(AnomalyLayerDepthAttr, layer.Depth),
                           new XAttribute(AnomalyLayerThicknessAttr, layer.Thickness));

            if (fileName != null)
                xelem.Add(new XElement(AnomalyFromFile,
                    new XAttribute(AnomalyFileType, "plain-text"),
                    new XAttribute(AnomalyFileName, fileName)));

            return xelem;
        }
    }
}
