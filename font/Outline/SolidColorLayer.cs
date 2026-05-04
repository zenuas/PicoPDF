using System.Drawing;

namespace OpenType.Outline;

public class SolidColorLayer : IOutline, IColorLayer, IHaveColorLayer
{
    public IColorLayer? ColorLayer { get => field = this; init; }
    public Color Color { get; init; }
}
