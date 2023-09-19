namespace PicoPDF.Pdf.Color;

public class DeviceCMYK : IColor
{
    public required double C { get; init; }
    public required double M { get; init; }
    public required double Y { get; init; }
    public required double K { get; init; }

    public string CreateColor(bool isstroke) => $"{C} {M} {Y} {K} {(isstroke ? "K" : "k")}";
}
