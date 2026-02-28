using Mina.Extension;
using OpenType;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PicoPDF.Pdf.Font;

public class CIDToUnicode : PdfObject
{
    public required IOpenTypeRequiredTables Font { get; init; }
    public required HashSet<int> Chars { get; init; }

    public override void DoExport(PdfExportOption option)
    {
        var bfchar = Chars
            .Order()
            .Select(x => (Char: x, String: char.ConvertFromUtf32(x)))
            .Select(x => $"        <{Font.CharToGID(x.Char):x4}> {PdfUtility.ToHexString(x.String, Encoding.BigEndianUnicode)}{(option.Debug ? $" %{x.String}" : "")}")
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
