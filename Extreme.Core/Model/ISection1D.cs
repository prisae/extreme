namespace Extreme.Core.Model
{
    public interface ISection1D<out T> where T : Layer1D
    {
        int NumberOfLayers { get; }
        decimal ZeroAirLevelAlongZ { get; }
        T this[int layerIndex] { get; }
    }
}
