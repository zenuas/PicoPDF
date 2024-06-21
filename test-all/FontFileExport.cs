using PicoPDF.Pdf.Font;

namespace PicoPDF.TestAll;

public static class FontFileExport
{
    public static void Export(FontRegister fontreg, Option opt)
    {
        var font = fontreg.GetOrNull(FontRegister.GetFontFilePath(opt.FontFileExport));
    }
}
