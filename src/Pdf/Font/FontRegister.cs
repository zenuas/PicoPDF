using Mina.Data;
using Mina.Extension;
using PicoPDF.OpenType;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PicoPDF.Pdf.Font;

public class FontRegister
{
    public Dictionary<string, PropertyGetSet<IOpenTypeHeader>> Fonts { get; init; } = [];
    public Dictionary<FontRequiredTables, PropertyGetSet<IOpenTypeRequiredTables>> LoadedFonts { get; init; } = [];

    public void RegistDirectory(LoadOption? opt = null, params string[] paths) => paths
        .Select(x => Directory.GetFiles(x, "*.*", SearchOption.AllDirectories))
        .Flatten()
        .Select(x => (Path: x, Extension: Path.GetExtension(x).ToUpper()))
        .Where(x => x.Extension is ".TTF" or ".TTC")
        .Each(x =>
        {
            if (x.Extension == ".TTC")
            {
                AddFontCollection(x.Path, opt);
            }
            else
            {
                AddFont(x.Path, opt);
            }
        });

    public void RegistDirectory(params string[] paths) => RegistDirectory(null, paths);

    public IOpenTypeRequiredTables? LoadRequiredTablesOrNull(string name)
    {
        var font = Fonts.GetValueOrDefault(name);
        if (font is null) return null;
        if (font.Value is IOpenTypeRequiredTables req) return req;

        var fontinfo = FontLoader.LoadRequiredTables(font.Value.Cast<FontTableRecords>());
        Fonts[name].Value = fontinfo;
        return fontinfo;
    }

    public IOpenTypeRequiredTables LoadRequiredTables(IFontPath path)
    {
        var name = GetFontFilePath(path);
        if (LoadRequiredTablesOrNull(name) is { } font) return font;

        if (path is FontCollectionPath)
        {
            AddFontCollection(path.Path, new() { ForceEmbedded = path.ForceEmbedded });
        }
        else
        {
            AddFont(path.Path, new() { ForceEmbedded = path.ForceEmbedded });
        }
        return LoadRequiredTablesOrNull(name).Try();
    }

    public IOpenTypeRequiredTables LoadRequiredTables(string name, bool forceEmbedded) => Path.GetExtension(name) != "" ?
        LoadRequiredTables(GetFontFilePath(name, forceEmbedded)) :
        LoadRequiredTablesOrNull(name).Try();

    public IOpenTypeRequiredTables LoadComplete(IOpenTypeRequiredTables otf) => otf is FontRequiredTables req ?
        (LoadedFonts.TryGetValue(req, out var x) ?
            x.Value :
            (LoadedFonts[req] = new() { Value = FontLoader.LoadComplete(req) }).Value
        ) :
        otf;

    public static string GetFontFilePath(IFontPath path) => path is FontCollectionPath fc ? $"{Path.GetFullPath(fc.Path)},{fc.Index}" : Path.GetFullPath(path.Path);

    public static IFontPath GetFontFilePath(string name, bool forceEmbedded)
    {
        var ext = Path.GetExtension(name).ToUpper();
        return ext.StartsWith(".TTC,") && int.TryParse(ext[5..], out var index)
            ? new FontCollectionPath { Path = Path.GetFullPath(name[0..^(ext.Length - 4)]), Index = index, ForceEmbedded = forceEmbedded }
            : new FontPath { Path = Path.GetFullPath(name), ForceEmbedded = forceEmbedded };
    }

    public bool Add(IOpenTypeHeader font)
    {
        var name = GetFontFilePath(font.Path);
        if (Fonts.ContainsKey(name)) return false;

        var r = new PropertyGetSet<IOpenTypeHeader>() { Value = font };
        Fonts.Add(name, r);
        font.Name.NameRecords
            .Where(x => x.NameRecord.NameID == 4)
            .Each(x => Fonts.TryAdd(x.Name, r));
        return true;
    }

    public void AddFont(string path, LoadOption? opt = null)
    {
        if (Objects.Catch(() => FontLoader.LoadTableRecords(path, opt), out var r) is null) Add(r);
    }

    public void AddFontCollection(string path, LoadOption? opt = null) => FontLoader.LoadTableRecordsCollection(path, opt).Each(x => Add(x));
}
