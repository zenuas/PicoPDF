namespace PicoPDF.Pdf.Drawing;

public record DeviceRGB(double R, double G, double B) : IColor
{
    public string CreateColor(bool isstroke) => $"{R} {G} {B} {(isstroke ? "RG" : "rg")}";
}
