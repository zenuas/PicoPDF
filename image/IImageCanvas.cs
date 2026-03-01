using System.Drawing;

namespace Image;

public interface IImageCanvas : IImage
{
    public Color[][] Canvas { get; init; }
}
