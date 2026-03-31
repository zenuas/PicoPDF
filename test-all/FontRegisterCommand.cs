using Mina.Command;
using PicoPDF.Pdf.Font;
using System.IO;

namespace PicoPDF.TestAll;

public abstract class FontRegisterCommand : ICommand
{
    [CommandOption("register-system-font")]
    public bool RegisterSystemFont { get; init; } = true;

    [CommandOption("register-user-font")]
    public string RegisterUserFont { get; init; } = "";

    public abstract void Run(string[] args);

    public IFontRegister CreateFontRegister(bool islock = false)
    {
        var fontreg = islock ? (IFontRegister)new FontRegisterLock() : new FontRegister();
        if (RegisterSystemFont) fontreg.RegisterDirectory([.. FontRegister.GetFontDirectories()]);
        if (RegisterUserFont != "" && !Directory.Exists(RegisterUserFont)) _ = fontreg.LoadComplete(RegisterUserFont);
        if (RegisterUserFont != "" && Directory.Exists(RegisterUserFont)) fontreg.RegisterDirectory(new OpenType.LoadOption() { ForceEmbedded = true }, RegisterUserFont);
        return fontreg;
    }
}
