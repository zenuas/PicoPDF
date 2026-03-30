using System.Numerics;

namespace OpenType.Outline;

public interface IEdge
{
    public Vector2 Start { get; init; }
    public Vector2 End { get; init; }
}
