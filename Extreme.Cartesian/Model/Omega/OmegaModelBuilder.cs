//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
﻿using System;
using System.IO;
using Extreme.Core;
using Extreme.Core.Model;

namespace Extreme.Cartesian.Model
{
    public class OmegaModelBuilder
    {
        public static OmegaModel LoadCartesianAndBuildOmegaModel(string path, double frequency)
        {
            var cartesianModel = SerializationManager.LoadModel(path);
            return BuildOmegaModel(cartesianModel, frequency);
        }

        
        public static OmegaModel BuildOmegaModel(CartesianModel cartesianModel, double frequency)
        {
            var omega = OmegaModelUtils.FrequencyToOmega(frequency);

            var section1D = ConvertSection1DIntoOmegaDependent(omega, cartesianModel);
            var anomaly = OmegaAnomaly.CreateFromCartesianAnomaly(omega, cartesianModel.Anomaly);

            return new OmegaModel(cartesianModel, section1D, anomaly, omega);
        }
        
        public static OmegaModel BuildOmegaModel(CartesianModel startModel, double[,,] sigma, double frequency)
        {
            var omega = OmegaModelUtils.FrequencyToOmega(frequency);

            var section1D = ConvertSection1DIntoOmegaDependent(omega, startModel);
            var anomaly = OmegaAnomaly.CreateFromCartesianAnomaly(omega, startModel.Anomaly, sigma);

            return new OmegaModel(startModel, section1D, anomaly, omega);
        }

        public static Section1D<IsotropyLayer> ConvertSection1DIntoOmegaDependent(double omega, CartesianModel model)
        {
            int length = model.Section1D.NumberOfLayers;
            var layersData = new IsotropyLayer[length];

            for (int i = 0; i < length; i++)
            {
                var layer = model.Section1D[i];

                layersData[i] = CreateIsotropy1DLayerDataFrom(omega, layer);
            }

            return new Section1D<IsotropyLayer>(model.Section1D.ZeroAirLevelAlongZ, layersData);
        }

        private static IsotropyLayer CreateIsotropy1DLayerDataFrom(double omega, Sigma1DLayer layer)
        {
            var zeta = OmegaModelUtils.ConvertSigmaToZeta(omega, layer.Sigma);

            return new IsotropyLayer(layer.Thickness, zeta);
        }
    }
}
