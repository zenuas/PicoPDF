using PicoPDF.Pdf.Font;
using System;
using System.Linq;

namespace PicoPDF.TestAll;

public static class NameRecordListTest
{
    public static void Preview(FontRegister fontreg, Option opt)
    {
        foreach (var kv in fontreg.Fonts.Where(x => opt.AllFontPreview || x.Key != FontRegister.GetFontFilePath(x.Value.Value.Path)))
        {
            var font = kv.Value.Value;
            var path = $"\"{Escape(kv.Value.Value.Path.Path)}\"";
            Console.WriteLine($"{path},PostScriptName,\"{Escape(font.PostScriptName)}\"");
            for (var i = 0; i < font.Name.NameRecords.Length; i++)
            {
                var name = font.Name.NameRecords[i];
                Console.WriteLine($"{path},NameRecords[{i}],\"{Escape(name.Name)}\",{name.NameRecord.PlatformID},{name.NameRecord.EncodingID},{name.NameRecord.LanguageID},{name.NameRecord.NameID}");
            }
        }
    }

    public static string Escape(string s) => s.Replace("\"", "\"\"");
}
