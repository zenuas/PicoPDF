using Mina.Command;
using Pdf.Font;
using System.IO;

namespace PicoPDF.TestAll;

public abstract class FontRegisterCommand : ICommand
{
    [CommandOption("register-system-font")]
    public bool RegisterSystemFont { get; init; } = false;

    [CommandOption("register-user-font")]
    public string RegisterUserFont { get; init; } = "test-case/font";

    public abstract void Run(string[] args);

    public IFontRegister CreateFontRegister(bool islock = false)
    {
        var fontreg = islock ? (IFontRegister)new FontRegisterLock() : new FontRegister();
        if (RegisterSystemFont) fontreg.RegisterDirectory([.. FontRegister.GetFontDirectories()]);
        if (RegisterUserFont != "" && !Directory.Exists(RegisterUserFont)) _ = fontreg.LoadFont(RegisterUserFont);
        if (RegisterUserFont != "" && Directory.Exists(RegisterUserFont)) fontreg.RegisterDirectory(new OpenType.LoadOption(), RegisterUserFont);
        return fontreg;
    }
}
