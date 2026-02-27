using System;

namespace PicoPDF.Pdf.Elements;

public class ElementDate : ElementValue
{
    public required DateTime Value { get; set; }

    public override string ToElementString()
    {
        switch (Value.Kind)
        {
            case DateTimeKind.Utc:
                return $"(D:{Value:yyyyMMddHHmmss}+00'00')";

            case DateTimeKind.Local:
                var offset = TimeZoneInfo.Local.GetUtcOffset(Value);
                var sign = offset >= TimeSpan.Zero ? "+" : "-";
                return $"(D:{Value:yyyyMMddHHmmss}{sign}{offset:hh\\'mm\\'})";

            case DateTimeKind.Unspecified:
                return $"(D:{Value:yyyyMMddHHmmss})";
        }
        throw new();
    }

    public static implicit operator ElementDate(DateTime x) => new() { Value = x };
}
