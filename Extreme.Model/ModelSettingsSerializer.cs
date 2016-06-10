using System.IO;
using System.Xml.Linq;
using Extreme.Cartesian.Model;
using Extreme.Core;
using Extreme.Model.SimpleCommemi3D;

namespace Extreme.Model
{
    public class ModelSettingsSerializer
    {
        #region Commemi
        public static bool IsModelCommemi(string path)
            => IsModel(path, "commemi");

        public static CommemiModelSettings LoadCommemiFromXml(string path)
        {
            var xdoc = XDocument.Load(path);
            var xsettings = xdoc.Element("ModelSettings");
            var mesh = ReadMeshParameters(xsettings);
            var mb = ReadManualBoundaries(xsettings);

            var xcond = xsettings.Element("Conductivity");
            var left = xcond.AttributeAsDouble("left");
            var right = xcond.AttributeAsDouble("right");

            return new CommemiModelSettings(mesh, mb)
                .WithAnomalySizeInMeters(xsettings.ElementAsIntOrNull("AnomalySizeInMeters") ?? 40000)
                .WithConductivities(left, right);
        }

        public static void SaveToXml(string path, CommemiModelSettings model)
        {
            var xdoc = new XDocument();
            var xelem = ToXElement(model, "commemi");

            xelem.Add(
                new XElement("AnomalySizeInMeters", model.AnomalySizeInMeters),
                new XElement("Conductivity",
                    new XAttribute("left", model.LeftConductivity),
                    new XAttribute("right", model.RightConductivity)));

            xdoc.Add(xelem);
            xdoc.Save(path);
        }

        #endregion

        #region Commemi3d3
        public static bool IsModelCommemi3D3(string path)
          => IsModel(path, "commemi3d3");

        public static Commemi3D3ModelSettings LoadCommemi3D3FromXml(string path)
        {
            var xdoc = XDocument.Load(path);
            var xsettings = xdoc.Element("ModelSettings");
            var mesh = ReadMeshParameters(xsettings);
            var mb = ReadManualBoundaries(xsettings);
            return new Commemi3D3ModelSettings(mesh, mb);
        }

        public static void SaveToXml(string path, Commemi3D3ModelSettings model)
        {
            var xdoc = new XDocument();
            xdoc.Add(ToXElement(model, "commemi3d3"));
            xdoc.Save(path);
        }

        #endregion

        #region OneBlock
        public static bool IsModelOneBlock(string path)
           => IsModel(path, "oneblock");

        public static void SaveToXml(string path, OneBlockModelSettings model)
        {
            var xdoc = new XDocument();
            var xelem = ToXElement(model, "oneblock");

            xelem.Add(
                new XElement("Conductivity", model.Conductivity),
                new XElement("AnomalySizeX", model.AnomalySizeX),
                new XElement("AnomalySizeY", model.AnomalySizeY),
                new XElement("AnomalySizeZ", model.AnomalySizeZ),
                new XElement("AnomalyStartDepth", model.AnomalyStartDepth));

            ModelWriter.SaveBackground(xelem, model.Section1D);

            xdoc.Add(xelem);
            xdoc.Save(path);
        }

        public static OneBlockModelSettings LoadOneBlockFromXml(string path)
        {
            var xdoc = XDocument.Load(path);
            var xsettings = xdoc.Element("ModelSettings");
            var mesh = ReadMeshParameters(xsettings);
            var mb = ReadManualBoundaries(xsettings);

            var model = new OneBlockModelSettings(mesh, mb)
                .WithConductivity(xsettings.ElementAsDoubleOrNull("Conductivity") ?? -1)
                .WithAnomalyStartDepth(xsettings.ElementAsDecimal("AnomalyStartDepth"))
                .WithAnomalySizeX(xsettings.ElementAsDecimal("AnomalySizeX"))
                .WithAnomalySizeY(xsettings.ElementAsDecimal("AnomalySizeY"))
                .WithAnomalySizeZ(xsettings.ElementAsDecimal("AnomalySizeZ"));

            model.Section1D = ModelReader.LoadBackground(xsettings);

            return model;
        }

        #endregion

        #region Naser

        public static bool IsModelNaser(string path)
        => IsModel(path, "naser");

        public static void SaveToXml(string path, NaserModelSettings model)
        {
            var xdoc = new XDocument();
            var xelem = ToXElement(model, "naser");

            xelem.Add(
                new XElement("TopConductivity", model.TopConductivity),
                new XElement("LeftConductivity", model.LeftConductivity),
                new XElement("RightConductivity", model.RightConductivity));

            xdoc.Add(xelem);
            xdoc.Save(path);
        }

        public static NaserModelSettings LoadNaserFromXml(string path)
        {
            var xdoc = XDocument.Load(path);
            var xsettings = xdoc.Element("ModelSettings");
            var mesh = ReadMeshParameters(xsettings);
            var mb = ReadManualBoundaries(xsettings);

            var model = new NaserModelSettings(mesh, mb)
                .WithTopConductivity(xsettings.ElementAsDoubleOrNull("TopConductivity") ?? 0.002)
                .WithLeftConductivity(xsettings.ElementAsDoubleOrNull("LeftConductivity") ?? 1)
                .WithRightConductivity(xsettings.ElementAsDoubleOrNull("RightConductivity") ?? 0.002);

            return model;
        }

        #endregion

        #region Common

        private static bool IsModel(string path, string name)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException(path);

            return XDocument.Load(path)
                .Element("ModelSettings")?
                .Attribute("model")?
                .Value == name;
        }

        private static XElement ToXElement(ModelSettings model, string modelType)
        {
            return new XElement("ModelSettings", new XAttribute("model", modelType),
                new XElement("Nx", model.Mesh.Nx),
                new XElement("Ny", model.Mesh.Ny),
                new XElement("Nz",
                    new XAttribute("isGeom", model.Mesh.UseGeometricStepAlongZ),
                    model.Mesh.UseGeometricStepAlongZ ?
                    new XAttribute("geomRation", model.Mesh.GeometricRation) :
                    null, model.Mesh.Nz),
                    ToXElement(model.ManualBoundaries));
        }

        private static XElement ToXElement(ManualBoundaries manualBoundaries)
        {
            if (manualBoundaries == ManualBoundaries.Auto)
                return null;

            return new XElement("ManualBoundaries",
                new XElement("StartX", manualBoundaries.StartX),
                new XElement("EndX", manualBoundaries.EndX),
                new XElement("StartY", manualBoundaries.StartY),
                new XElement("EndY", manualBoundaries.EndY));
        }

        private static MeshParameters ReadMeshParameters(XElement xelem)
        {
            var nx = xelem.ElementAsIntOrNull("Nx") ?? 0;
            var ny = xelem.ElementAsIntOrNull("Ny") ?? 0;

            var xnz = xelem.Element("Nz");
            var nz = xelem.ElementAsIntOrNull("Nz") ?? 0;
            var geomRation = xnz.AttributeAsDoubleOrNull("geomRation") ?? 1.06;
            var nzIsGeometric = xnz.AttributeAsBoolOrNull("isGeom") ?? false;

            var mesh = new MeshParameters(nx, ny, nz)
            {
                GeometricRation = geomRation,
                UseGeometricStepAlongZ = nzIsGeometric,
            };

            return mesh;
        }

        private static ManualBoundaries ReadManualBoundaries(XElement xelem)
        {
            var xmb = xelem.Element("ManualBoundaries");

            if (xmb == null)
                return ManualBoundaries.Auto;

            return new ManualBoundaries
            (
                startX: xmb.ElementAsDecimal("StartX"),
                startY: xmb.ElementAsDecimal("StartY"),
                endX: xmb.ElementAsDecimal("EndX"),
                endY: xmb.ElementAsDecimal("EndY")
            );
        } 
        
        #endregion
    }
}
