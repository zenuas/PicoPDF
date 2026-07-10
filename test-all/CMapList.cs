using Mina.Command;
using System;

namespace PicoPDF.TestAll;

public class CMapList : FontRegisterCommand
{
    [CommandOption("all-font-preview")]
    public bool AllFontPreview { get; init; } = false;

    public override void Run(string[] args)
    {
        var fontreg = CreateFontRegister();

        foreach (var (name, _) in fontreg.GetFonts(AllFontPreview))
        {
            var font = fontreg.LoadFont(name);
            foreach (var encoding in font.CMap.EncodingRecords)
            {
                Console.WriteLine($"{name},PlatformID={encoding.Key.PlatformID},EncodingID={encoding.Key.EncodingID},Format={encoding.Value.Format}");
            }
        }
    }
}
