using PicoPDF.Pdf.Drawing;
using PicoPDF.Pdf.Function;

namespace PicoPDF.Pdf.Shading;

public class AxialShading : PdfObject, IShading
{
    public ShadingTypes ShadingType { get; init; } = ShadingTypes.AxialShading;
    public string ColorSpace { get; init; } = "/DeviceRGB";
    public required (IPoint X0, IPoint Y0, IPoint X1, IPoint Y1) Coords { get; init; }
    public (float T0, float T1) Domain { get; init; } = (0.0f, 1.0f);
    public required IFunction Function { get; init; }
    public (bool B0, bool B1) Extend { get; init; } = (false, false);
}
