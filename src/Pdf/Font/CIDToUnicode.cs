using Mina.Extension;
using PicoPDF.OpenType;
using System.Collections.Generic;
using System.Linq;

namespace PicoPDF.Pdf.Font;

public class CIDToUnicode : PdfObject
{
    public required IOpenTypeRequiredTables Font { get; init; }
    public required HashSet<int> Chars { get; init; }

    public override void DoExport(PdfExportOption option)
    {
        var bfchar = Chars
            .Order()
            .Select(x => $"        <{Font.CharToGID(x):x4}> <{x:x4}>{(option.Debug ? $" %{PdfUtility.ToStringByChars([x])}" : "")}")
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
