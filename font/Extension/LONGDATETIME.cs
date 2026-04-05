using System;

namespace OpenType.Extension;

public readonly struct LONGDATETIME
{
    public required long Value { get; init; }

    public DateTime? ToDateTime()
    {
        try
        {
            return BaseTime.AddSeconds(Value);
        }
        catch
        {
            return null;
        }
    }

    public static readonly DateTime BaseTime = new(1904, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public static implicit operator LONGDATETIME(DateTime v) => new() { Value = (long)((v.ToUniversalTime() - BaseTime).TotalSeconds) };
    public static implicit operator LONGDATETIME(long v) => new() { Value = v };
    public static implicit operator DateTime?(LONGDATETIME v) => v.ToDateTime();
}
