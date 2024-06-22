using PicoPDF.OpenType;
using PicoPDF.Pdf.Font;
using System.IO;

namespace PicoPDF.TestAll;

public static class FontFileExport
{
    public static void Export(FontRegister fontreg, Option opt)
    {
        var font = fontreg.Get(FontRegister.GetFontFilePath(opt.FontFileExport));
        using var stream = new MemoryStream();
        FontExport.Export(font, stream);
    }
}
