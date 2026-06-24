using Mina.Extension;
using Pdf.Font;
using System;
using System.Linq;

namespace Pdf.Documents;

public partial class Document
{
    public Func<string, FontEmbeds, Type0Font> CreateFontCache()
    {
        var fontcache = PdfObjects.OfType<Type0Font>().ToDictionary(x => x.Name, x => x);
        return (name, embed) =>
        {
            var namekey = $"{name};{embed}";
            if (fontcache.TryGetValue(namekey, out var value)) return value;
            var x = Type0Font.Create($"F{fontcache.Count}", FontRegister.LoadComplete(name), embed);
            AddFont(x);
            fontcache.Add(namekey, x);
            return x;
        };
    }

    public void AddFont(IFont font) => PdfObjects.Add(font.Cast<PdfObject>());
}
