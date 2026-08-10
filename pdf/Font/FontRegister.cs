using Mina.Data;
using Mina.Extension;
using OpenType;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace Pdf.Font;

public class FontRegister : IFontRegister
{
    public Dictionary<string, PropertyGetSet<IOpenTypeHeader>> Fonts { get; init; } = [];

    public void RegisterDirectory(LoadOption? option = null, params string[] paths) => paths
        .Where(Directory.Exists)
        .Select(x => Directory.GetFiles(x, "*.*", SearchOption.AllDirectories))
        .Flatten()
        .Select(x => (Path: x, Extension: Path.GetExtension(x).ToUpper(CultureInfo.InvariantCulture)))
        .Where(x => x.Extension is ".TTF" or ".TTC" or ".OTF")
        .Each(x =>
        {
            try
            {
                if (x.Extension == ".TTC")
                {
                    AddFontCollection(x.Path, option);
                }
                else
                {
                    AddFont(x.Path, option);
                }
            }
            catch
            {
                // ignore loading errors
            }
        });

    public void RegisterDirectory(params string[] paths) => RegisterDirectory(null, paths);

    public IOpenTypeFont LoadFont(string name, LoadOption? option = null)
    {
        var opt = option ?? new();
        var keyname = $"{name};vert={opt.UseVertical}";
        if (Fonts.TryGetValue(keyname, out var x)) return x.Value.Cast<IOpenTypeFont>();

        if (!Fonts.TryGetValue(name, out var fontdata))
        {
            var path = GetFontFilePathValue(name);
            var fullpath = path.GetPath();
            if (Fonts.TryGetValue($"{fullpath};vert={opt.UseVertical}", out var x2)) return x2.Value.Cast<IOpenTypeFont>();

            if (path is FontCollectionPath ttc)
            {
                AddFontCollection(ttc.Path, opt);
            }
            else
            {
                AddFont(name, opt);
            }
            fontdata = Fonts[name];
        }
        var font = FontLoader.LoadFont(fontdata.Value, opt);
        var prop = new PropertyGetSet<IOpenTypeHeader>() { Value = font };
        Fonts.Add($"{fontdata.Value.Path.GetPath()};vert={opt.UseVertical}", prop);
        font.Name.NameRecords
            .Where(x => x.NameRecord.NameID == NameIDs.FullFontName)
            .Each(x => Fonts.TryAdd($"{x.Name};vert={opt.UseVertical}", prop));
        return font;
    }

    public (string Name, IOpenTypeHeader Font)[] GetFonts(bool include_alternative_font = false) => [.. Fonts
        .Where(x => include_alternative_font || x.Key == x.Value.Value.Path.GetPath())
        .Select(x => (x.Key, x.Value.Value))];

    public static IFontPath GetFontFilePathValue(string name)
    {
        var ext = Path.GetExtension(name).ToUpper(CultureInfo.InvariantCulture);
        return ext.StartsWith(".TTC,", StringComparison.Ordinal) && int.TryParse(ext[5..], out var index)
            ? new FontCollectionPath { Path = Path.GetFullPath(name[0..^(ext.Length - 4)]), Index = index }
            : new FontPath { Path = Path.GetFullPath(name) };
    }

    public bool Add(IOpenTypeHeader font)
    {
        var name = font.Path.GetPath();
        if (Fonts.ContainsKey(name)) return false;

        var prop = new PropertyGetSet<IOpenTypeHeader>() { Value = font };
        Fonts.Add(name, prop);
        font.Name.NameRecords
            .Where(x => x.NameRecord.NameID == NameIDs.FullFontName)
            .Each(x => Fonts.TryAdd(x.Name, prop));
        return true;
    }

    public void AddFont(string path, LoadOption? option = null) => Add(FontLoader.LoadTableRecords(path, option));

    public void AddFontCollection(string path, LoadOption? option = null) => FontLoader.LoadTableRecordsCollection(path, option).Each(x => Add(x));

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
