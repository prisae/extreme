using System.Collections.Generic;
using System.Xml.Linq;
using Extreme.Cartesian.Project;

namespace Extreme.Cartesian.Forward
{
    public class ForwardProjectSerializer
    {
        public static void Save(ForwardProject project, string fileName)
            => ToXDocument(project).Save(fileName);

        public static ForwardProject Load(string fileName)
            => FromXDocument(XDocument.Load(fileName));

        public static XDocument ToXDocument(ForwardProject project)
        {
            var serializer = new ProjectWriter(
                project: project.Extreme,
                settingsWriters: new Dictionary<string, IProjectSettingsWriter>
                {
                    ["Forward"] = new ForwardSettingsWriter()
                });

            return serializer.ToXDocument();
        }

        public static ForwardProject FromXDocument(XDocument xdoc)
        {
            var serializer = new ProjectReader(
                settingsReaders: new Dictionary<string, IProjectSettingsReader>
                {
                    ["Forward"] = new ForwardSettingsReader()
                });

            return ForwardProject.NewFrom(serializer.FromXDocument(xdoc));
        }
    }
}