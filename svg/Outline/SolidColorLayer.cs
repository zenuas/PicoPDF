using System.Drawing;

namespace Svg.Outline;

public class SolidColorLayer : IOutline, IColorLayer, IHaveColorLayer
{
    public IColorLayer? ColorLayer => this;
    public Color Color { get; init; }
}
