using OpenType;
using System.Threading;

namespace PicoPDF.Pdf.Font;

public class FontRegisterLock : IFontRegister
{
    private readonly FontRegister FontRegister = new();
    private readonly Lock LockObject = new();

    public void RegisterDirectory(LoadOption? opt = null, params string[] paths)
    {
        lock (LockObject)
        {
            FontRegister.RegisterDirectory(opt, paths);
        }
    }

    public void RegisterDirectory(params string[] paths)
    {
        lock (LockObject)
        {
            FontRegister.RegisterDirectory(paths);
        }
    }

    public IOpenTypeFont LoadComplete(string name)
    {
        lock (LockObject)
        {
            return FontRegister.LoadComplete(name);
        }
    }

    public (string Name, IOpenTypeHeader Font)[] GetFonts(bool include_alternative_font = false)
    {
        lock (LockObject)
        {
            return FontRegister.GetFonts(include_alternative_font);
        }
    }
}
