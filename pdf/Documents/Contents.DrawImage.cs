using Pdf.Drawing;
using Pdf.Operation;
using Pdf.XObject.Image;

namespace Pdf.Documents;

public partial class Contents
{
    public static DrawImage CreateDrawImageOperation(double x, double y, IImageXObject image, double zoomwidth = 1.0, double zoomheight = 1.0) => CreateDrawImageOperation(
        new PointValue(x),
        new PointValue(y),
        image,
        zoomwidth,
        zoomheight
    );

    public static DrawImage CreateDrawImageOperation(IPoint x, IPoint y, IImageXObject image, double zoomwidth = 1.0, double zoomheight = 1.0) => new()
    {
        X = x,
        Y = y,
        Image = image,
        ZoomWidth = zoomwidth,
        ZoomHeight = zoomheight,
    };
}
