using PicoPDF.Pdf.Documents.Security;
using PicoPDF.Pdf.Extension;
using System;
using System.Text;

namespace PicoPDF.Pdf.Elements;

public class ElementDate : ElementValue
{
    public required DateTime Value { get; set; }

    public override string ToElementString(IConverter? converter)
    {
        string d;
        switch (Value.Kind)
        {
            case DateTimeKind.Utc:
                d = $"D:{Value:yyyyMMddHHmmss}+00'00'";
                break;

            case DateTimeKind.Local:
                var offset = TimeZoneInfo.Local.GetUtcOffset(Value);
                var sign = offset >= TimeSpan.Zero ? "+" : "-";
                d = $"D:{Value:yyyyMMddHHmmss}{sign}{offset:hh\\'mm\\'}";
                break;

            case DateTimeKind.Unspecified:
                d = $"D:{Value:yyyyMMddHHmmss}";
                break;

            default:
                throw new();
        }
        return converter is { } ? d.ToEncryptString(Encoding.ASCII, converter) : $"({d})";
    }

    public static implicit operator ElementDate(DateTime x) => new() { Value = x };
}
