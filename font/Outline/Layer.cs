namespace OpenType.Outline;

public class Layer : IOutline
{
    public required IOutline[] Surfaces { get; init; }
}
