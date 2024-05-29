using Mina.Extension;
using PicoPDF.TrueType;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PicoPDF.Pdf.Font;

public class FontRegister
{
    public Dictionary<string, TrueTypeFontInfo> Fonts { get; init; } = [];

    public void RegistDirectory(string path) => RegistDirectory([path]);

    public void RegistDirectory(IEnumerable<string> paths)
    {
        paths
            .Select(x => Directory.GetFiles(x, "*.*", SearchOption.AllDirectories))
            .Flatten()
            .Select(x => (Path: x, Extension: Path.GetExtension(x).ToUpper()))
            .Where(x => x.Extension.In(".TTF", ".TTC"))
            .Select(x => x.Extension == ".TTF" ? [TrueTypeFontLoader.Load(x.Path)] : TrueTypeFontLoader.LoadCollection(x.Path))
            .Flatten()
            .Each(x => Fonts.TryAdd(x.PostScriptName, x));
    }

    public TrueTypeFontInfo? GetOrNull(string name)
    {
        var font = Fonts.GetValueOrDefault(name);
        if (font is null || font.Loaded) return font;

        TrueTypeFontLoader.DelayLoad(font);
        return font;
    }
}
