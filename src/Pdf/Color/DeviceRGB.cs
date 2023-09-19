namespace PicoPDF.Pdf.Color;

public class DeviceRGB : IColor
{
    public required double R { get; init; }
    public required double G { get; init; }
    public required double B { get; init; }

    public string CreateColor(bool isstroke) => $"{R} {G} {B} {(isstroke ? "RG" : "rg")}";
}
