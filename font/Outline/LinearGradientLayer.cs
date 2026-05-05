using System.Drawing;
using System.Numerics;

namespace OpenType.Outline;

public class LinearGradientLayer : IOutline, IColorLayer, IHaveColorLayer
{
    public IColorLayer? ColorLayer { get => field = this; init; }
    public required Vector2 XY1 { get; init; }
    public required Vector2 XY2 { get; init; }
    public required (float Offset, Color StopColor)[] StopColors { get; init; }
    public required SpreadMethods SpreadMethod { get; init; }
}
