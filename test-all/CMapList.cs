using Mina.Command;
using PicoPDF.Pdf.Font;
using System;

namespace PicoPDF.TestAll;

public class CMapList : FontRegisterCommand
{
    [CommandOption("all-font-preview")]
    public bool AllFontPreview { get; init; } = false;

    public override void Run(string[] args)
    {
        var fontreg = CreateFontRegister();

        foreach (var (name, font) in fontreg.GetFonts(AllFontPreview))
        {
            var font_complete = fontreg.LoadComplete(FontRegister.GetFontFilePath(font.Path));
            foreach (var encoding in font_complete.CMap.EncodingRecords)
            {
                Console.WriteLine($"{name},PlatformID={encoding.Key.PlatformID},EncodingID={encoding.Key.EncodingID},Format={encoding.Value.Format}");
            }
        }
    }
}
