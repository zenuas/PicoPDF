using Mina.Command;
using System;

namespace PicoPDF.TestAll;

public class FontList : FontRegisterCommand
{
    [CommandOption("all-font-preview")]
    public bool AllFontPreview { get; init; } = false;

    public override void Run(string[] args)
    {
        var fontreg = CreateFontRegister();

        foreach (var (name, font) in fontreg.GetFonts(AllFontPreview))
        {
            Console.WriteLine($"{name},\"{font.Path.GetPath()}\",Offset.SfntVersion=0x{font.Offset.SfntVersion:x8}");
        }
    }
}
