using OpenType;

namespace PicoPDF.Pdf.Font;

public interface IFontRegister
{
    public void RegisterDirectory(LoadOption? opt = null, params string[] paths);
    public void RegisterDirectory(params string[] paths);
    public IOpenTypeFont LoadComplete(string name);
    public (string Name, IOpenTypeHeader Font)[] GetFonts(bool include_alternative_font = false);
}
