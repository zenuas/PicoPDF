namespace Pdf.Drawing;

public record DeviceGray(double Gray) : IColor
{
    public string CreateColor(bool isstroke) => $"{Gray} {(isstroke ? "G" : "g")}";
}
