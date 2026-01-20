using PicoPDF.OpenType;

namespace PicoPDF.Pdf.Font;

public interface IFontRegister
{
    public void RegisterDirectory(LoadOption? opt = null, params string[] paths);
    public void RegisterDirectory(params string[] paths);
    public IOpenTypeRequiredTables LoadRequiredTables(string name, bool forceEmbedded);
    public IOpenTypeRequiredTables LoadComplete(IOpenTypeRequiredTables font);
}
