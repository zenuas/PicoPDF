using Mina.Command;
using Mina.Extension;
using PicoPDF.Pdf.Font;
using System;
using System.Linq;

namespace PicoPDF.TestAll;

public class NameRecordList : FontRegisterCommand
{
    [CommandOption("all-font-preview")]
    public bool AllFontPreview { get; init; } = false;

    public override void Run(string[] args)
    {
        var fontreg = CreateFontRegister().Cast<FontRegister>();

        foreach (var kv in fontreg.Fonts.Where(x => AllFontPreview || x.Key == FontRegister.GetFontFilePath(x.Value.Value.Path)))
        {
            var font = kv.Value.Value;

            var path = $"\"{Escape(font.Path.Path)}\"";
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
