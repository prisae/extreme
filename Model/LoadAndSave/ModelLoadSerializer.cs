﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Extreme.Cartesian.Model;
using Extreme.Core;

namespace Extreme.Cartesian.Model
{
    public class ModelLoadSerializer : ModelSerializer
    {
        public static CartesianModel Load(string path)
        {
            var xdoc = XDocument.Load(path);

            return CartesianModelFromX(xdoc, path);
        }

        public static CartesianModel CartesianModelFromX(XContainer xelem, string path = "")
        {
            var xmodel = xelem.Element(Model);

            if (xmodel == null)
                throw new CartesianModelLoadException(Extreme.Cartesian.Properties.ExceptionMessages.SimpleModelLoader_WrongXmlFormat);

            if (xmodel.Attribute(ModelVersionAttr).Value != SecondVersion)
                throw new CartesianModelLoadException("Wrong version");

            var lateral = LoadLateralDimensions(xmodel);
            var section1D = LoadBackground(xmodel);
            var anomalies = LoadAnomalies(path, lateral, xmodel);

            return new CartesianModel(lateral, section1D, anomalies);
        }

        private static LateralDimensions LoadLateralDimensions(XElement xElement)
        {
            var lateral = LateralDimensionsFromXElement(xElement);

            if (lateral == default(LateralDimensions))
                throw new CartesianModelLoadException("missing lateral dimensions");

            return lateral;
        }

        public static LateralDimensions LateralDimensionsFromXElement(XElement xElement)
        {
            var ld = xElement.Element(LateralDimensionsItem);

            if (ld == null)
                return default(LateralDimensions);

            int nx = ld.AttributeAsInt(LateralDimensionsNxAttr);
            int ny = ld.AttributeAsInt(LateralDimensionsNyAttr);

            var dx = ld.AttributeAsDecimal(LateralDimensionsDxAttr);
            var dy = ld.AttributeAsDecimal(LateralDimensionsDyAttr);

            return new LateralDimensions(nx, ny, dx, dy);
        }

        private static CartesianSection1D LoadBackground(XElement xmodel)
        {
            var background = xmodel.Element(BackgroundSection);

            if (background == null)
                throw new CartesianModelLoadException(Extreme.Cartesian.Properties.ExceptionMessages.SimpleModelLoader_BackgroundLayersAreNotPresented);

            var zeroLevel = background.AttributeAsDecimalOrNull(BackgroundZeroLevelAttr) ?? 0;
            var xlayers = background.Elements(BackgroundLayer).ToArray();

            var dataLayers = new List<Sigma1DLayer>();

            foreach (var xlayer in xlayers)
            {
                var thickness = xlayer.AttributeAsDecimal(BackgroundLayerThicknessAttr);
                var sigmaReal = xlayer.AttributeAsFloat(BackgroundLayerSigmaRealAttr);

                dataLayers.Add(new Sigma1DLayer(thickness, sigmaReal));
            }

            return new CartesianSection1D(zeroLevel, dataLayers.ToArray());
        }

        private static CartesianAnomaly LoadAnomalies(string path, LateralDimensions lateral, XElement xmodel)
        {
            var xanomaly = xmodel.Element(AnomalySection);

            var size = new Size2D(lateral.Nx, lateral.Ny);
            var anomalyLayers = new List<CartesianAnomalyLayer>();

            if (xanomaly == null)
                return new CartesianAnomaly(size, anomalyLayers);

            foreach (var xlayer in xanomaly.Elements(AnomalyLayer))
            {
                var loadedLayer = ReadAnomalyLayer(xlayer, size, path);
                anomalyLayers.Add(loadedLayer);
            }

            return new CartesianAnomaly(size, anomalyLayers);
        }

        private static CartesianAnomalyLayer ReadAnomalyLayer(XElement xlayer, Size2D size, string path)
        {
            var depth = xlayer.AttributeAsDecimal(AnomalyLayerDepthAttr);
            var thickness = xlayer.AttributeAsDecimal(AnomalyLayerThicknessAttr);

            var anomalyLayer = new CartesianAnomalyLayer(depth, thickness)
            {
                UnderlyingXml = xlayer
            };

            //var xfromFile = xlayer.Element(AnomalyFromFile);
            //if (xfromFile != null)
            //    LoadAnomalyValuesFromFile(xfromFile, anomalyLayer, path);

            //var xapplique = xlayer.Element(AnomalyApplique);
            //if (xapplique != null)
            //    LoadAnomalyValuesFromApplique(xapplique, anomalyLayer);

            return anomalyLayer;
        }

        public static void PopulateAnomalyLayer(string path, Size2D localSize, CartesianAnomalyLayer layer)
        {
            var xlayer = layer.UnderlyingXml;

            var depth = xlayer.AttributeAsDecimal(AnomalyLayerDepthAttr);
            var thickness = xlayer.AttributeAsDecimal(AnomalyLayerThicknessAttr);

            if (layer.Depth != depth) throw new InvalidOperationException();
            if (layer.Thickness != thickness) throw new InvalidOperationException();

            layer.Sigma = new double[localSize.Nx, localSize.Ny];

            var xfromFile = xlayer.Element(AnomalyFromFile);
            if (xfromFile != null)
                LoadAnomalyValuesFromFile(xfromFile, layer, path);

            var xapplique = xlayer.Element(AnomalyApplique);
            if (xapplique != null)
                LoadAnomalyValuesFromApplique(xapplique, layer);
        }

        public static void LoadAnomalyValuesFromApplique(XElement xapplique, CartesianAnomalyLayer anomalyLayer)
        {
            AnomalyLoaderUtils.ParseApplique(xapplique.Value, anomalyLayer);
        }

        private static void LoadAnomalyValuesFromFile(XElement xFromFile, CartesianAnomalyLayer anomalyLayer, string path)
        {
            var fileType = xFromFile.Attribute(AnomalyFileType).Value;
            var fileName = xFromFile.Attribute(AnomalyFileName);

            if (fileType == "plain-text" && fileName != null)
            {
                var fullPath = Path.Combine(Path.GetDirectoryName(path), fileName.Value);

                using (var lr = new LinesReader(fullPath))
                {
                    AnomalyLoaderUtils.ReadAnomalyDataFromPlainText(lr, anomalyLayer.Sigma);
                }
            }
        }
    }
}
