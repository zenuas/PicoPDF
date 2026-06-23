using Mina.Extension;
using OpenType;
using Pdf.Extension;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pdf.Font;

public class CIDToUnicode : PdfObject
{
    public required IOpenTypeFont Font { get; init; }
    public required HashSet<int> Chars { get; init; }

    public override void BeforeExport(PdfExportOption option)
    {
        var bfchar = Chars
            .Order()
            .Select(x => (Char: x, String: char.ConvertFromUtf32(x)))
            .Select(x => $"        <{Font.CharToGID(x.Char):x4}> {x.String.ToHexString(Encoding.BigEndianUnicode)}{(option.Debug ? $" %{x.String}" : "")}")
            .Join("\n");

        var writer = GetWriteStream(option.CMapStreamDeflate);
        writer.Write($"""
/CIDInit /ProcSet findresource begin
  12 dict begin
    begincmap
      /CIDSystemInfo << /Registry (Adobe) /Ordering (UCS) /Supplement 0 >> def
      /CMapName /Adobe-Identity-UCS def
      /CMapType 2 def
      1 begincodespacerange
        <0000> <FFFF>
      endcodespacerange
      {Chars.Count} beginbfchar
{bfchar}
      endbfchar
    endcmap
    CMapName currentdict /CMap defineresource pop
  end
end
""");
        writer.Flush();
    }
}
