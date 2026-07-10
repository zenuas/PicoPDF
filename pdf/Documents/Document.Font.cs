using Pdf.Font;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pdf.Documents;

public partial class Document
{
    public List<IFont> Fonts { get; init; } = [];

    public Func<string, FontEmbeds, Type0Font> CreateFontCache()
    {
        var fontcache = Fonts.OfType<Type0Font>().ToDictionary(x => x.Name, x => x);
        return (name, embed) =>
        {
            var namekey = $"{name};{embed}";
            if (fontcache.TryGetValue(namekey, out var value)) return value;
            var x = Type0Font.Create($"F{fontcache.Count}", FontRegister.LoadFont(name), embed);
            Fonts.Add(x);
            fontcache.Add(namekey, x);
            return x;
        };
    }
}
