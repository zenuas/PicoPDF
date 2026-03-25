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
            try
            {
                var font = fontreg.LoadComplete(FontRegister.GetFontFilePath(kv.Value.Value.Path));
                Console.WriteLine($"{kv.Key},\"{FontRegister.GetFontFilePath(font.Path)}\",Offset.SfntVersion=0x{font.Offset.SfntVersion:x8},OS2.FsType=0x{font.OS2?.FsType:x4}");
            }
            catch
            {
                Console.WriteLine($"{kv.Key},\"{FontRegister.GetFontFilePath(kv.Value.Value.Path)}\",Offset.SfntVersion=0x{kv.Value.Value.Offset.SfntVersion:x8}");
            }
        }
    }
}
