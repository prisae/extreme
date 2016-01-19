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

            ModelReader.PopulateAnomaly(path, model.Anomaly);

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
                return ModelReader.LoadWithoutAnomalyData(path);

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

            UpdateAnomalyLocalSize(mpi, model.Anomaly);

            for (int k = 0; k < model.Anomaly.Layers.Count; k++)
            {
                var layer = model.Anomaly.Layers[k];
                var xFromFile = layer.UnderlyingXml.Element(ModelSerializer.AnomalyFromFile);

                var fileType = xFromFile?.Attribute(ModelSerializer.AnomalyFileType).Value;
                var fileName = xFromFile?.Attribute(ModelSerializer.AnomalyFileName);

                if (fileType == "plain-text" && fileName != null)
                {
                    var fullPath = Path.Combine(Path.GetDirectoryName(path), fileName.Value);
                    DistributedLoadModelMaster(mpi, fullPath, model, k);
                }
            }

            return model;
        }

        private static unsafe void DistributedLoadModelMaster(Mpi mpi, string fullPath, CartesianModel model, int k)
        {
            using (var lr = new LinesReader(fullPath))
            {
                for (int rank = 0; rank < mpi.Size; rank++)
                {
                    var nxLength = mpi.CalcLocalHalfNxLength(rank, model.LateralDimensions.Nx);
                    var sigma = new double[nxLength, model.Anomaly.LocalSize.Ny, 1];
                    var sendLength = nxLength * model.Anomaly.LocalSize.Ny;

                    if (nxLength == 0)
                        continue;

                    AnomalyLoaderUtils.ReadAnomalyDataFromPlainText(lr, sigma, 0);

                    if (rank == 0)
                    {
                        for (int i = 0; i < nxLength; i++)
                            for (int j = 0; j < model.Anomaly.LocalSize.Ny; j++)
                                model.Anomaly.Sigma[i, j, k] = sigma[i, j, 0];
                    }
                    else if (nxLength != 0)
                    {
                        fixed (double* data = &sigma[0, 0, 0])
                            mpi.Send(data, sendLength, Mpi.Double, rank, 0, Mpi.CommWorld);
                    }
                }
            }
        }

        private static unsafe CartesianModel DistributedLoadModelSlave(Mpi mpi, string path)
        {
            var model = LoadModelWithoutExternFiles(path);
            var anomaly = model.Anomaly;
            var nxLength = mpi.CalcLocalHalfNxLength(model.LateralDimensions.Nx);
            var recvLength = nxLength * anomaly.LocalSize.Ny;

            UpdateAnomalyLocalSize(mpi, anomaly);

            if (nxLength != 0)
                for (int k = 0; k < anomaly.Layers.Count; k++)
                {
                    fixed (double* data = &anomaly.Sigma[0, 0, k])
                    {
                        var actualSourse = 0;
                        mpi.Recv(data, recvLength, Mpi.Double, 0, 0, Mpi.CommWorld, out actualSourse);
                    }
                }

            return model;
        }


        private static void UpdateAnomalyLocalSize(Mpi mpi, CartesianAnomaly anomaly)
        {
            var localSize = new Size2D(mpi.CalcLocalHalfNxLength(anomaly.LocalSize.Nx), anomaly.LocalSize.Ny);
            anomaly.ChangeLocalSize(localSize);
            anomaly.CreateSigma();
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
