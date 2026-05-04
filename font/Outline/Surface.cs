namespace OpenType.Outline;

public class Surface : IOutline, IHaveColorLayer
{
    public required IEdge[] Edges { get; init; }
    public IColorLayer? ColorLayer { get; init; } = null;
}
