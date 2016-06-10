using System.Xml.Linq;
using Extreme.Cartesian.Project;

namespace Extreme.Cartesian.Forward
{
    public class ForwardSettingsWriter: IProjectSettingsWriter
    {
        public XElement ToXElement(ProjectSettings settings)
        {
            var forwardSettings = settings as ForwardSettings;

            return new XElement(settings.Name,
               new XElement("Residual", forwardSettings?.Residual),
               new XElement("InnerBufferLength", forwardSettings?.InnerBufferLength),
               new XElement("OuterBufferLength", forwardSettings?.OuterBufferLength),
               new XElement("MaxRepeatsNumber", forwardSettings?.MaxRepeatsNumber),
               new XElement("NumberOfHankels", forwardSettings?.NumberOfHankels));
        }
    }
}
