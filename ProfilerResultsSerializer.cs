using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Extreme.Cartesian.Model;
using Extreme.Core;

namespace Profiling
{
    public static class ProfilerResultsSerializer
    {
        public static void SaveWithModel(this ProfilerRecord[] profilerRecord, string path, CartesianModel model)
        {
            SaveWithModel(path, model, profilerRecord);
        }

        public static void SaveWithModel(string path, CartesianModel model, ProfilerRecord[] profilerRecord)
        {
            var xdoc = new XDocument();

            xdoc.Add(new XElement("ProfilingResults",
                ModelSaveSerializer.ToXElement(model),
                new XElement("Records",
                    profilerRecord.Select(ToXElement))));

            xdoc.Save(path);
        }

        private static XElement ToXElement(ProfilerRecord record)
        {
            return new XElement("R", 
                new XAttribute("code", record.Code),
                new XAttribute("type", record.IsStart? "start" : "end"),
                record.TimeStamp);
        }

        public static CartesianModel LoadModel(string path)
        {
            var xdoc = XDocument.Load(path);

            var xresults = xdoc.Element("ProfilingResults");

            if (xresults == null)
                throw new FileLoadException("wrong format");

            return ModelLoadSerializer.CartesianModelFromX(xresults);
        }

        public static ProfilerRecord[] LoadRecords(string path)
        {
            var xdoc = XDocument.Load(path);

            var xresults = xdoc.Element("ProfilingResults");

            if (xresults == null)
                throw new FileLoadException("wrong format");

            var xrecords = xresults.Element("Records");

            if (xrecords == null)
                throw new FileLoadException("wrong format");
            
            var xrecs = xrecords.Elements("R");

            return xrecs.Select(ToProfileRecord).ToArray();
        }

        private static ProfilerRecord ToProfileRecord(XElement xelem)
        {
            var code = xelem.AttributeAsInt("code");
            bool start = xelem.Attribute("type").Value == "start";
            var value  = xelem.ValueAsLong();

            return new ProfilerRecord(code, value, start);
        }
    }
}
