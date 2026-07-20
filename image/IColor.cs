using System.Drawing;

namespace Image;

public interface IColor
{
    public IColor AlphaBlend();
    public Color ToColor();
}
