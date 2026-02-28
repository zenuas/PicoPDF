using Mina.Data;
using Mina.Extension;
using OpenType;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace PicoPDF.Pdf.Font;

public class FontRegister : IFontRegister
{
    public Dictionary<string, PropertyGetSet<IOpenTypeHeader>> Fonts { get; init; } = [];
    public Dictionary<FontRequiredTables, PropertyGetSet<IOpenTypeRequiredTables>> LoadedFonts { get; init; } = [];

    public void RegisterDirectory(LoadOption? opt = null, params string[] paths) => paths
        .Where(Directory.Exists)
        .Select(x => Directory.GetFiles(x, "*.*", SearchOption.AllDirectories))
        .Flatten()
        .Select(x => (Path: x, Extension: Path.GetExtension(x).ToUpper()))
        .Where(x => x.Extension is ".TTF" or ".TTC")
        .Each(x =>
        {
            try
            {
                if (x.Extension == ".TTC")
                {
                    AddFontCollection(x.Path, opt);
                }
                else
                {
                    AddFont(x.Path, opt);
                }
            }
            catch
            {
                // ignore loading errors
            }
        });

    public void RegisterDirectory(params string[] paths) => RegisterDirectory(null, paths);

    public IOpenTypeRequiredTables? LoadRequiredTablesOrNull(string name)
    {
        var font = Fonts.GetValueOrDefault(name);
        if (font is null) return null;
        if (font.Value is IOpenTypeRequiredTables otr) return otr;

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
            AddFontCollection(path.Path);
        }
        else
        {
            AddFont(path.Path);
        }
        return LoadRequiredTablesOrNull(name).Try();
    }

    public IOpenTypeRequiredTables LoadRequiredTables(string name) => Path.GetExtension(name) != "" ?
        LoadRequiredTables(GetFontFilePath(name)) :
        LoadRequiredTablesOrNull(name).Try();

    public IOpenTypeRequiredTables LoadComplete(IOpenTypeRequiredTables font) => font is FontRequiredTables req ?
        (LoadedFonts.TryGetValue(req, out var x) ?
            x.Value :
            (LoadedFonts[req] = new() { Value = FontLoader.LoadComplete(req) }).Value
        ) :
        font;

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

    public void AddFont(string path, LoadOption? opt = null) => Add(FontLoader.LoadTableRecords(path, opt));

    public void AddFontCollection(string path, LoadOption? opt = null) => FontLoader.LoadTableRecordsCollection(path, opt).Each(x => Add(x));

    public static IEnumerable<string> GetFontDirectories() => [.. GetSystemFontDirectories(), .. GetUserFontDirectories()];

    public static IEnumerable<string> GetSystemFontDirectories()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            yield return Environment.ExpandEnvironmentVariables(@"%SystemRoot%\Fonts");
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            yield return "/System/Library/Fonts";
            yield return "/Library/Fonts";
        }
        else
        {
            yield return "/usr/share/fonts";
            yield return "/usr/local/share/fonts";
        }
    }

    public static IEnumerable<string> GetUserFontDirectories()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            yield return Environment.ExpandEnvironmentVariables(@"%UserProfile%\AppData\Local\Microsoft\Windows\Fonts");
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            yield return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Library/Fonts");
        }
        else
        {
            yield return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".fonts");
            yield return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local/share/fonts");
        }
    }
}
