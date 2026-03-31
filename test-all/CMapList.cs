using Mina.Command;
using Mina.Extension;
using PicoPDF.Pdf.Font;
using System;
using System.Linq;

namespace PicoPDF.TestAll;

public class CMapList : FontRegisterCommand
{
    [CommandOption("all-font-preview")]
    public bool AllFontPreview { get; init; } = false;

    public override void Run(string[] args)
    {
        var fontreg = CreateFontRegister().Cast<FontRegister>();

        foreach (var kv in fontreg.Fonts.Where(x => AllFontPreview || x.Key == FontRegister.GetFontFilePath(x.Value.Value.Path)))
        {
            var font = fontreg.LoadComplete(FontRegister.GetFontFilePath(kv.Value.Value.Path));
            foreach (var encoding in font.CMap.EncodingRecords)
            {
                Console.WriteLine($"{kv.Key},PlatformID={encoding.Key.PlatformID},EncodingID={encoding.Key.EncodingID},Format={encoding.Value.Format}");
            }
        }
    }
}
