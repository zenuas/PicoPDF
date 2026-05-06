namespace OpenType.Outline;

public class Layer : IOutline, IHaveColorLayer
{
    public required IOutline[] Surfaces { get; init; }
    public IColorLayer? ColorLayer { get; init; } = null;
}
