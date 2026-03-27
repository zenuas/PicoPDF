using System.Drawing;

namespace OpenType.Outline;

public interface IEdge
{
    public Point Start { get; init; }
    public Point End { get; init; }
}
