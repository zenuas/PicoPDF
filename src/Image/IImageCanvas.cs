using System.Drawing;

namespace PicoPDF.Image;

public interface IImageCanvas : IImage
{
    public Color[][] Canvas { get; init; }
}
