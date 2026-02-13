using PicoPDF.Pdf.Font;
using System;
using System.Linq;

namespace PicoPDF.TestAll;

public static class CMapListTest
{
    public static void Preview(FontRegister fontreg, Option opt)
    {
        foreach (var kv in fontreg.Fonts.Where(x => opt.AllFontPreview || x.Key != FontRegister.GetFontFilePath(x.Value.Value.Path)))
        {
            var font = fontreg.LoadRequiredTables(kv.Key);
            foreach (var encoding in font.CMap.EncodingRecords)
            {
                Console.WriteLine($"{kv.Key},PlatformID={encoding.Key.PlatformID},EncodingID={encoding.Key.EncodingID},Format={encoding.Value.Format}");
            }
        }
    }
}
