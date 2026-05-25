using System.Drawing;
using System.Numerics;

namespace Svg.Outline;

public class RadialGradientLayer : IOutline, IColorLayer, IHaveColorLayer
{
    public IColorLayer? ColorLayer { get => field = this; init; }
    public required Vector2 Cxy { get; init; }
    public required Vector2 Fxy { get; init; }
    public required float Fr { get; init; }
    public required float R { get; init; }
    public required (float Offset, Color StopColor)[] StopColors { get; init; }
    public required SpreadMethods SpreadMethod { get; init; }
    public required Matrix3x2 GradientTransform { get; init; }
}
