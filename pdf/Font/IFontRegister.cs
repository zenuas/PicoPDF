using OpenType;

namespace Pdf.Font;

public interface IFontRegister
{
    public void RegisterDirectory(LoadOption? option = null, params string[] paths);
    public void RegisterDirectory(params string[] paths);
    public IOpenTypeFont LoadFont(string name, LoadOption? option = null);
    public (string Name, IOpenTypeHeader Font)[] GetFonts(bool include_alternative_font = false);
}
