using PicoPDF.Pdf.Drawing;
using PicoPDF.Pdf.Operation;
using PicoPDF.Pdf.XObject.Image;

namespace PicoPDF.Pdf.Documents;

public partial class Contents
{
    public void DrawImage(double x, double y, IImageXObject image, double zoomwidth = 1.0, double zoomheight = 1.0) => Operations.Add(CreateDrawImageOperation(x, y, image, zoomwidth, zoomheight));

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
