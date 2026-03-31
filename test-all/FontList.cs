using Mina.Command;
using Mina.Extension;
using PicoPDF.Pdf.Font;
using System;
using System.Linq;

namespace PicoPDF.TestAll;

public class FontList : FontRegisterCommand
{
    [CommandOption("all-font-preview")]
    public bool AllFontPreview { get; init; } = false;

    public override void Run(string[] args)
    {
        var fontreg = CreateFontRegister().Cast<FontRegister>();

        foreach (var kv in fontreg.Fonts.Where(x => AllFontPreview || x.Key != FontRegister.GetFontFilePath(x.Value.Value.Path)))
        {
            var font = kv.Value.Value;
            Console.WriteLine($"{kv.Key},\"{FontRegister.GetFontFilePath(font.Path)}\",Offset.SfntVersion=0x{font.Offset.SfntVersion:x8}");
        }
    }
}
