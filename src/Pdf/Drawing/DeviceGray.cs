namespace PicoPDF.Pdf.Drawing;

public record class DeviceGray(double Gray) : IColor
{
    public string CreateColor(bool isstroke) => $"{Gray} {(isstroke ? "G" : "g")}";
}
