using Mina.Extension;
using System.Collections.Generic;

namespace PicoPDF.Pdf.Font;

public interface IFontChars
{
    public HashSet<int> Chars { get; init; }

    public void WriteString(string s) => s.ToUtf32CharArray().Each(x => Chars.Add(x));
}
