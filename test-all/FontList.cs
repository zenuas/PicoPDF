using Mina.Command;
using PicoPDF.Pdf.Font;
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
            Console.WriteLine($"{name},\"{FontRegister.GetFontFilePath(font.Path)}\",Offset.SfntVersion=0x{font.Offset.SfntVersion:x8}");
        }
    }
}
