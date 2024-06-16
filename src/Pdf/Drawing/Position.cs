namespace PicoPDF.Pdf.Drawing;

public record struct Position(double Left, double Top, double Right, double Bottom)
{
    public readonly double Height => Top > 0 ? Bottom - Top : -Top + Bottom;
    public readonly double Width => Left > 0 ? Right - Left : -Left + Right;
}
