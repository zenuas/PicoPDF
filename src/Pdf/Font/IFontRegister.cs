using OpenType;
using OpenType.Tables;

namespace PicoPDF.Pdf.Font;

public interface IFontRegister
{
    public void RegisterDirectory(LoadOption? opt = null, params string[] paths);
    public void RegisterDirectory(params string[] paths);
    public IOpenTypeRequiredTables LoadRequiredTables(string name);
    public IOpenTypeFont LoadComplete(IOpenTypeRequiredTables font);
}
