using System;
using System.Diagnostics;
using System.IO;
using System.Xml.Linq;
using Extreme.Cartesian.Forward;
using Extreme.Cartesian.Model;
using Extreme.Core;
using Extreme.Parallel;


namespace Extreme.Cartesian.Model
{
    public class SerializationManager
    {
        private const string FirstVersion = @"1.0";
        private const string SecondVersion = @"2.0";

        private const string Model = @"CartesianModel";
        private const string ModelVersionAttr = @"version";

        public static CartesianModel LoadModel(string path)
        {
            var model = LoadModelWithoutExternFiles(path);

            foreach (var layer in model.Anomaly.Layers)
                ModelLoadSerializer.PopulateAnomalyLayer(path, model.Anomaly.LocalSize, layer);

            return model;
        }

        private static CartesianModel LoadModelWithoutExternFiles(string path)
        {
            var xdoc = XDocument.Load(path);

            var xmodel = xdoc.Element(Model);

            if (xmodel == null)
                throw new CartesianModelLoadException(Extreme.Cartesian.Properties.ExceptionMessages.SimpleModelLoader_WrongXmlFormat);

            var version = xmodel.Attribute(ModelVersionAttr).Value;

            if (version == FirstVersion)
                throw new CartesianModelLoadException(Extreme.Cartesian.Properties.ExceptionMessages.SimpleModelLoader_UnknownVersion);

            if (version == SecondVersion)
                return ModelLoadSerializer.Load(path);

            throw new CartesianModelLoadException(Extreme.Cartesian.Properties.ExceptionMessages.SimpleModelLoader_UnknownVersion);
        }

        public static CartesianModel DistributedLoadModel(Mpi mpi, string path)
        {
            if (mpi == null || !mpi.IsParallel)
                return LoadModel(path);

            if (mpi.IsMaster)
                return DistributedLoadModelMaster(mpi, path);
            else
                return DistributedLoadModelSlave(mpi, path);
        }


        private static CartesianModel DistributedLoadModelMaster(Mpi mpi, string path)
        {
            var model = LoadModelWithoutExternFiles(path);

            UpdateAnomalyLocalSize(mpi, model);

            foreach (var layer in model.Anomaly.Layers)
            {
                var xFromFile = layer.UnderlyingXml.Element(ModelSerializer.AnomalyFromFile);

                var fileType = xFromFile?.Attribute(ModelSerializer.AnomalyFileType).Value;
                var fileName = xFromFile?.Attribute(ModelSerializer.AnomalyFileName);

                if (fileType == "plain-text" && fileName != null)
                {
                    var fullPath = Path.Combine(Path.GetDirectoryName(path), fileName.Value);
                    DistributedLoadModelMaster(mpi, fullPath, model, layer);
                }
            }

            return model;
        }

        private static unsafe void DistributedLoadModelMaster(Mpi mpi, string fullPath, CartesianModel model, CartesianAnomalyLayer layer)
        {
            using (var lr = new LinesReader(fullPath))
            {
                for (int i = 0; i < mpi.Size; i++)
                {
                    var nxLength = mpi.CalcLocalHalfNxLength(i, model.LateralDimensions.Nx);
                    var sigma = new double[nxLength, model.Anomaly.LocalSize.Ny];
                    var sendLength = nxLength * model.Anomaly.LocalSize.Ny;
                    AnomalyLoaderUtils.ReadAnomalyDataFromPlainText(lr, sigma);
                    if (i == 0)
                        layer.Sigma = sigma;
                    else if (nxLength != 0)
                        fixed (double* data = &sigma[0, 0])
                            mpi.Send(data, sendLength, Mpi.Double, i, 0, Mpi.CommWorld);
                }
            }
        }

        private static unsafe CartesianModel DistributedLoadModelSlave(Mpi mpi, string path)
        {
            var model = LoadModelWithoutExternFiles(path);
            var nxLength = mpi.CalcLocalHalfNxLength(model.LateralDimensions.Nx);
            var recvLength = nxLength * model.Anomaly.LocalSize.Ny;

            UpdateAnomalyLocalSize(mpi, model);

            foreach (var layer in model.Anomaly.Layers)
                layer.Sigma = new double[nxLength, model.LateralDimensions.Ny];

            if (nxLength != 0)
                foreach (var layer in model.Anomaly.Layers)
                {
                    var sigma = layer.Sigma;

                    fixed (double* data = &sigma[0, 0])
                    {
                        var actualSourse = 0;
                        mpi.Recv(data, recvLength, Mpi.Double, 0, 0, Mpi.CommWorld, out actualSourse);
                    }
                }

            return model;
        }


        private static void UpdateAnomalyLocalSize(Mpi mpi, CartesianModel model)
        {
            var localSize = new Size2D(mpi.CalcLocalHalfNxLength(model.Anomaly.LocalSize.Nx), model.Anomaly.LocalSize.Ny);
            model.Anomaly.ChangeLocalSize(localSize);
        }

        private static object LoadPartialAnomaly(int nxStart, int nxLength)
        {
            throw new NotImplementedException();
        }



        public static void SaveModel(string path, CartesianModel model)
        {
            ModelWriter.Save(path, model);
        }

        public static void SaveModelWithPlaintTextAnomaly(string path, CartesianModel model)
        {
            ModelWriter.SaveWithPlainTextAnomaly(path, model);
        }
    }
}
