using Mina.Extension;
using System.Collections.Generic;

namespace PicoPDF.Pdf.Font;

public interface IFontChars
{
    public HashSet<int> Chars { get; init; }

    public void WriteString(string s) => PdfUtility.ToUtf32CharArray(s).Each(x => Chars.Add(x));
}
