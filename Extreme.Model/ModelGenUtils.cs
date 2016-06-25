//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
﻿using System;
using Extreme.Model;
using Extreme.Model.SimpleCommemi3D;
using Extreme.Cartesian.Forward;
using Extreme.Cartesian.Model;
using Extreme.Core.Model;
using Extreme.Parallel;

namespace Extreme.Model
{
    public static class ModelGenUtils
    {
        public static CartesianModel LoadCartesianModelWithoutAnomalyData(string modelFile)
        {
            try
            {
                if (ModelSettingsSerializer.IsModelCommemi(modelFile))
                {
                    var settings = ModelSettingsSerializer.LoadCommemiFromXml(modelFile);
                    return CreateModelWithoutAnomalyData(settings);
                }

                if (ModelSettingsSerializer.IsModelCommemi3D3(modelFile))
                {
                    var settings = ModelSettingsSerializer.LoadCommemi3D3FromXml(modelFile);
                    return CreateModelWithoutAnomalyData(settings);
                }

                if (ModelSettingsSerializer.IsModelOneBlock(modelFile))
                {
                    var settings = ModelSettingsSerializer.LoadOneBlockFromXml(modelFile);
                    return CreateModelWithoutAnomalyData(settings);
                }

                if (ModelSettingsSerializer.IsModelNaser(modelFile))
                {
                    var settings = ModelSettingsSerializer.LoadNaserFromXml(modelFile);
                    return CreateModelWithoutAnomalyData(settings);
                }


                return ModelReader.LoadWithoutAnomalyData(modelFile);
            }

            catch (Exception e)
            {
                throw new InvalidOperationException($"Can't load model {modelFile}", e);
            }
        }

        public static CartesianModel LoadCartesianModel(string modelFile, Mpi mpi = null)
        {
            try
            {
                if (ModelSettingsSerializer.IsModelCommemi(modelFile))
                {
                    var settings = ModelSettingsSerializer.LoadCommemiFromXml(modelFile);
                    return CreateCommemiModel(settings, mpi);
                }

                if (ModelSettingsSerializer.IsModelCommemi3D3(modelFile))
                {
                    var settings = ModelSettingsSerializer.LoadCommemi3D3FromXml(modelFile);
                    return CreateCommemi3D3Model(settings, mpi);
                }

                if (ModelSettingsSerializer.IsModelOneBlock(modelFile))
                {
                    var settings = ModelSettingsSerializer.LoadOneBlockFromXml(modelFile);
                    return CreateOneBlockModel(settings, mpi);
                }

                if (ModelSettingsSerializer.IsModelNaser(modelFile))
                {
                    var settings = ModelSettingsSerializer.LoadNaserFromXml(modelFile);
                    return CreateNaserModel(settings, mpi);
                }


                return SerializationManager.DistributedLoadModel(mpi, modelFile);
            }

            catch (Exception e)
            {
                throw new InvalidOperationException($"Can't load model {modelFile}", e);
            }
        }

        public static CartesianModel CreateCommemiModel(CommemiModelSettings settings, Mpi mpi = null)
        {
            var creater = new SimpleCommemi3DModelCreater(settings.AnomalySizeInMeters);
            return GenerateModel(mpi, settings, () =>
                    creater.CreateNonMeshedModel(settings.LeftConductivity, settings.RightConductivity));
        }

        public static CartesianModel CreateCommemi3D3Model(Commemi3D3ModelSettings settings, Mpi mpi = null)
        {
            return GenerateModel(mpi, settings, Commemi3D3ModelCreater.CreateNonMeshedModel);
        }

        public static CartesianModel CreateOneBlockModel(OneBlockModelSettings settings, Mpi mpi = null)
        {
            var creater = new OneBlockModelCreater(settings);
            return GenerateModel(mpi, settings, creater.CreateNonMeshedModel);
        }

        public static CartesianModel CreateNaserModel(NaserModelSettings settings, Mpi mpi = null)
        {
            var creater = new NaserModelCreater(settings);
            return GenerateModel(mpi, settings, creater.CreateNonMeshedModel);
        }


        public static CartesianModel CreateModelWithoutAnomalyData(CommemiModelSettings settings)
        {
            var creater = new SimpleCommemi3DModelCreater(settings.AnomalySizeInMeters);
            return GenerateModelWithoutAnomalyData(settings, () =>
                    creater.CreateNonMeshedModel(settings.LeftConductivity, settings.RightConductivity));
        }

        public static CartesianModel CreateModelWithoutAnomalyData(Commemi3D3ModelSettings settings)
        {
            return GenerateModelWithoutAnomalyData(settings, Commemi3D3ModelCreater.CreateNonMeshedModel);
        }

        public static CartesianModel CreateModelWithoutAnomalyData(OneBlockModelSettings settings)
        {
            var creater = new OneBlockModelCreater(settings);
            return GenerateModelWithoutAnomalyData(settings, creater.CreateNonMeshedModel);
        }

        public static CartesianModel CreateModelWithoutAnomalyData(NaserModelSettings settings)
        {
            var creater = new NaserModelCreater(settings);
            return GenerateModelWithoutAnomalyData(settings, creater.CreateNonMeshedModel);
        }

        private static CartesianModel GenerateModelWithoutAnomalyData(ModelSettings settings, Func<NonMeshedModel> genNonMeshed)
        {
            var nonMeshed = genNonMeshed();
            var converter = new NonMeshedToCartesianModelConverter(nonMeshed);
            var mesh = settings.Mesh;
            converter.SetManualBoundaries(settings.ManualBoundaries);

            var cartesian = converter.ConvertWithoutAnomalyData(mesh);
            return cartesian;
        }

        private static CartesianModel GenerateModel(Mpi mpi, ModelSettings settings, Func<NonMeshedModel> genNonMeshed)
        {
            var nonMeshed = genNonMeshed();
            var converter = new NonMeshedToCartesianModelConverter(nonMeshed);
            var mesh = settings.Mesh;
            converter.SetManualBoundaries(settings.ManualBoundaries);
                
            var nxStart = 0;
            var nxLength = mesh.Nx;

            if (mpi != null && mpi.IsParallel)
            {
                nxStart = mpi.CalcLocalHalfNxStart(mesh.Nx);
                nxLength = mpi.CalcLocalHalfNxLength(mesh.Nx);
            }

            var cartesian = converter.Convert(mesh, nxStart, nxLength);
            return cartesian;
        }
    }
}
