using PicoPDF.OpenType;
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

    public IOpenTypeRequiredTables LoadRequiredTables(string name)
    {
        lock (LockObject)
        {
            return FontRegister.LoadRequiredTables(name);
        }
    }

    public IOpenTypeRequiredTables LoadComplete(IOpenTypeRequiredTables font)
    {
        lock (LockObject)
        {
            return FontRegister.LoadComplete(font);
        }
    }
}
