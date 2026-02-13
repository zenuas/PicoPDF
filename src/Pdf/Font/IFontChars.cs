using Mina.Extension;
using System.Collections.Generic;

namespace PicoPDF.Pdf.Font;

public interface IFontChars
{
    public HashSet<char> Chars { get; init; }

    public void WriteString(string s) => s.Each(x => Chars.Add(x));
}
