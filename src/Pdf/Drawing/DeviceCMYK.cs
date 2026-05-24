namespace PicoPDF.Pdf.Drawing;

public record struct DeviceCMYK(double C, double M, double Y, double K) : IColor
{
    public string CreateColor(bool isstroke) => $"{C} {M} {Y} {K} {(isstroke ? "K" : "k")}";
}
