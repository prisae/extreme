using System.Xml.Linq;

namespace Extreme.Cartesian.Project
{
    public interface IProjectSettingsReader
    {
        ProjectSettings FromXElement(XElement xelem);
    }
}
