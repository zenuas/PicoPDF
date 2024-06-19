using Mina.Extension;
using PicoPDF.OpenType;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PicoPDF.Pdf.Font;

public class FontRegister
{
    public Dictionary<string, OpenType.IFont> Fonts { get; init; } = [];

    public void RegistDirectory(params string[] paths) => paths
        .Select(x => Directory.GetFiles(x, "*.*", SearchOption.AllDirectories))
        .Flatten()
        .Select(x => (Path: x, Extension: Path.GetExtension(x).ToUpper()))
        .Where(x => x.Extension is ".TTF" or ".TTC")
        .Select(x => x.Extension == ".TTF" ? [FontLoader.Load(x.Path)] : FontLoader.LoadCollection(x.Path))
        .Flatten()
        .Each(x => Fonts.TryAdd(x.PostScriptName, x));

    public FontInfo? GetOrNull(string name)
    {
        var font = Fonts.GetValueOrDefault(name);
        if (font is null) return null;
        if (font is FontInfo fi) return fi;

        var fontinfo = FontLoader.DelayLoad(font.Cast<FontLoading>());
        Fonts[name] = fontinfo;
        return fontinfo;
    }
}
