using PicoPDF.Pdf.Font;
using System;
using System.Linq;

namespace PicoPDF.TestAll;

public static class FontListTest
{
    public static void Preview(FontRegister fontreg, Option opt)
    {
        foreach (var kv in fontreg.Fonts.Where(x => opt.AllFontPreview || x.Key != FontRegister.GetFontFilePath(x.Value.Value.Path)))
        {
            Console.WriteLine($"{kv.Key},\"{FontRegister.GetFontFilePath(kv.Value.Value.Path)}\"");
        }
    }
}
