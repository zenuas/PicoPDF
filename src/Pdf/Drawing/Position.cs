namespace PicoPDF.Pdf.Drawing;

public struct Position
{
    public double Left;
    public double Top;
    public double Right;
    public double Bottom;

    public readonly double Height => Top > 0 ? Bottom - Top : -Top + Bottom;
    public readonly double Width => Left > 0 ? Right - Left : -Left + Right;
}
