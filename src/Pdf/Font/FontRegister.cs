﻿using Mina.Data;
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

    public IOpenTypeRequiredTables? GetOrNull(string name)
    {
        var font = Fonts.GetValueOrDefault(name);
        if (font is null) return null;
        if (font.Value is IOpenTypeRequiredTables req) return req;

        var fontinfo = FontLoader.DelayLoad(font.Value.Cast<FontLoading>());
        Fonts[name].Value = fontinfo;
        return fontinfo;
    }

    public IOpenTypeRequiredTables? GetCompleteOrNull(string name)
    {
        var font = GetOrNull(name);
        if (font is null) return null;
        if (font is not FontRequiredTables req) return font;

        var fontinfo = FontLoader.DelayLoadComplete(req);
        Fonts[name].Value = fontinfo;
        return fontinfo;
    }

    public IOpenTypeRequiredTables Get(IFontPath path)
    {
        var name = GetFontFilePath(path);
        if (GetOrNull(name) is { } font) return font;

        if (path is FontCollectionPath)
        {
            AddFontCollection(path.Path);
        }
        else
        {
            AddFont(path.Path);
        }
        return GetOrNull(name).Try();
    }

    public IOpenTypeRequiredTables GetComplete(IFontPath path)
    {
        _ = Get(path);
        var name = GetFontFilePath(path);
        return GetCompleteOrNull(name).Try();
    }

    public IOpenTypeRequiredTables Get(string name) => Path.GetExtension(name) != "" ?
        Get(GetFontFilePath(name)) :
        GetOrNull(name).Try();

    public IOpenTypeRequiredTables GetComplete(string name) => Path.GetExtension(name) != "" ?
        GetComplete(GetFontFilePath(name)) :
        GetCompleteOrNull(name).Try();

    public static string GetFontFilePath(IFontPath path) => path is FontCollectionPath fc ? $"{Path.GetFullPath(fc.Path)},{fc.Index}" : Path.GetFullPath(path.Path);

    public static IFontPath GetFontFilePath(string name)
    {
        var ext = Path.GetExtension(name).ToUpper();
        if (ext.StartsWith(".TTC,") && int.TryParse(ext[5..], out var index))
        {
            return new FontCollectionPath { Path = Path.GetFullPath(name[0..^(ext.Length - 4)]), Index = index };
        }
        return new FontPath { Path = Path.GetFullPath(name) };
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

    public void AddFont(string path) => Add(FontLoader.Load(path));

    public void AddFontCollection(string path) => FontLoader.LoadCollection(path).Each(x => Add(x));
}
