using OpenType;
using System.Threading;

namespace Pdf.Font;

public class FontRegisterLock : IFontRegister
{
    protected readonly FontRegister FontRegister = new();
    protected readonly Lock LockObject = new();

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

    public IOpenTypeFont LoadFont(string name)
    {
        lock (LockObject)
        {
            return FontRegister.LoadFont(name);
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
