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

    public void RegistDirectory(params string[] paths) => paths
        .Select(x => Directory.GetFiles(x, "*.*", SearchOption.AllDirectories))
        .Flatten()
        .Select(x => (Path: x, Extension: Path.GetExtension(x).ToUpper()))
        .Where(x => x.Extension is ".TTF" or ".TTC")
        .Each(x =>
        {
            if (x.Extension == ".TTC")
            {
                AddFontCollection(x.Path);
            }
            else
            {
                AddFont(x.Path);
            }
        });

    public IOpenTypeRequiredTables? LoadRequiredTablesOrNull(string name)
    {
        var font = Fonts.GetValueOrDefault(name);
        if (font is null) return null;
        if (font.Value is IOpenTypeRequiredTables req) return req;

        var fontinfo = FontLoader.LoadRequiredTables(font.Value.Cast<FontTableRecords>());
        Fonts[name].Value = fontinfo;
        return fontinfo;
    }

    public IOpenTypeRequiredTables? LoadCompleteOrNull(string name)
    {
        var font = LoadRequiredTablesOrNull(name);
        if (font is null) return null;
        if (font is not FontRequiredTables req) return font;

        var fontinfo = FontLoader.LoadComplete(req);
        Fonts[name].Value = fontinfo;
        return fontinfo;
    }

    public IOpenTypeRequiredTables LoadRequiredTables(IFontPath path)
    {
        var name = GetFontFilePath(path);
        if (LoadRequiredTablesOrNull(name) is { } font) return font;

        if (path is FontCollectionPath)
        {
            AddFontCollection(path.Path);
        }
        else
        {
            AddFont(path.Path);
        }
        return LoadRequiredTablesOrNull(name).Try();
    }

    public IOpenTypeRequiredTables LoadComplete(IFontPath path)
    {
        _ = LoadRequiredTables(path);
        var name = GetFontFilePath(path);
        return LoadCompleteOrNull(name).Try();
    }

    public IOpenTypeRequiredTables LoadRequiredTables(string name) => Path.GetExtension(name) != "" ?
        LoadRequiredTables(GetFontFilePath(name)) :
        LoadRequiredTablesOrNull(name).Try();

    public IOpenTypeRequiredTables LoadComplete(string name) => Path.GetExtension(name) != "" ?
        LoadComplete(GetFontFilePath(name)) :
        LoadCompleteOrNull(name).Try();

    public static string GetFontFilePath(IFontPath path) => path is FontCollectionPath fc ? $"{Path.GetFullPath(fc.Path)},{fc.Index}" : Path.GetFullPath(path.Path);

    public static IFontPath GetFontFilePath(string name)
    {
        var ext = Path.GetExtension(name).ToUpper();
        return ext.StartsWith(".TTC,") && int.TryParse(ext[5..], out var index)
            ? new FontCollectionPath { Path = Path.GetFullPath(name[0..^(ext.Length - 4)]), Index = index }
            : new FontPath { Path = Path.GetFullPath(name) };
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

    public void AddFont(string path) => Add(FontLoader.LoadTableRecords(path));

    public void AddFontCollection(string path) => FontLoader.LoadTableRecordsCollection(path).Each(x => Add(x));
}
