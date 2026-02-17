namespace PicoPDF.Pdf.Drawing;

public record struct FontBox(double Ascender, double Descender, double Width)
{
    public readonly double Height => Ascender > 0 ? Descender - Ascender : -Ascender + Descender;
}
