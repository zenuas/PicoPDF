namespace PicoPDF.Pdf.Color;

public record class DeviceRGB(double R, double G, double B) : IColor
{
    public string CreateColor(bool isstroke) => $"{R} {G} {B} {(isstroke ? "RG" : "rg")}";
}
