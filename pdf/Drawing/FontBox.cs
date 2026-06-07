namespace Pdf.Drawing;

public record FontBox(double Ascender, double Descender, double LineGap, double Width)
{
    public double Height => Ascender > 0 ? Descender - Ascender : -Ascender + Descender;
}
