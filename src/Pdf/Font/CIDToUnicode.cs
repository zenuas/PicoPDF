using Mina.Extension;
using PicoPDF.TrueType;
using System.Linq;
using System.Text;

namespace PicoPDF.Pdf.Font;

public class CIDToUnicode : PdfObject
{
    public required TrueTypeFontInfo Font { get; init; }

    public override void DoExport(PdfExportOption option)
    {
        var bfchar = Font.CMap4Cache
            .OrderBy(x => x.Key)
            .Select(x => $"        <{x.Value:x4}> <{Encoding.BigEndianUnicode.GetBytes(x.Key.ToString()).Select(x => $"{x:x2}").Join()}>{(option.Debug ? $" %{x.Key}" : "")}")
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
      {Font.CMap4Cache.Count} beginbfchar
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
