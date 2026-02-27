using System;
using System.Diagnostics.CodeAnalysis;

namespace PicoPDF.Loader.Section;

public class PageSize : ISpanParsable<PageSize>, IComparable<PageSize>
{
    public int Width { get; init; }
    public int Height { get; init; }

    public PageSize(PageSizes size) => (Width, Height) = GetVerticalPageSize(size);

    public PageSize(int width, int height)
    {
        Width = width;
        Height = height;
    }

    public static PageSize Parse(string s) => Parse(s.AsSpan(), null);

    public static PageSize Parse(string s, IFormatProvider? provider) => Parse(s.AsSpan(), provider);

    public static PageSize Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
    {
        _ = TryParse(s, provider, out var result);
        return result!;
    }

    public static bool TryParse(string s, [MaybeNullWhen(false)] out PageSize result) => TryParse(s.AsSpan(), null, out result);

    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out PageSize result)
    {
        result = default;
        return s is not null && TryParse(s.AsSpan(), provider, out result);
    }

    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, [MaybeNullWhen(false)] out PageSize result)
    {
        result = default;
        if (s.Length == 0) return false;
        if (!char.IsNumber(s[0]))
        {
            if (Enum.TryParse<PageSizes>(s, out var sizes))
            {
                result = new PageSize(sizes);
                return true;
            }
        }
        var separator = s.IndexOf(',');
        if (separator < 0) return false;
        if (int.TryParse(s[0..separator], provider, out var width) && int.TryParse(s[(separator + 1)..], provider, out var height)) result = new PageSize(width, height);
        return result is not null;
    }

    public int CompareTo(PageSize? other) => other is null ? 1
        : Width != other.Width ? Width - other.Width
        : Height != other.Height ? Height - other.Height
        : 0;

    public (int Width, int Height) GetPageSize(Orientation orientation) => orientation == Orientation.Vertical ? (Width, Height) : (Height, Width);

    public static (int Width, int Height) GetPageSize(PageSizes size, Orientation orientation)
    {
        var (width, height) = GetVerticalPageSize(size);
        return orientation == Orientation.Vertical ? (width, height) : (height, width);
    }

    public static (int Width, int Height) GetVerticalPageSize(PageSizes size)
    {
        return size switch
        {
            PageSizes.A0 => (2384, 3370),
            PageSizes.A1 => (1684, 2384),
            PageSizes.A2 => (1191, 1684),
            PageSizes.A3 => (842, 1191),
            PageSizes.A4 => (595, 842),
            PageSizes.A5 => (420, 595),

            PageSizes.B0 => (2835, 4008),
            PageSizes.B1 => (2004, 2835),
            PageSizes.B2 => (1417, 2004),
            PageSizes.B3 => (1001, 1417),
            PageSizes.B4 => (709, 1001),
            PageSizes.B5 => (499, 709),

            _ => (0, 0),
        };
    }
}
