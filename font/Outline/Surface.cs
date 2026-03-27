namespace OpenType.Outline;

public class Surface : IOutline
{
    public required IEdge[] Edges { get; init; }
}
