using System.Xml.Linq;

namespace Extreme.Cartesian.Project
{
    public interface IProjectSettingsWriter
    {
        XElement ToXElement(ProjectSettings settings);
    }
}
