using Mina.Extension;
using PicoPDF.Pdf.Drawing;
using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;

namespace PicoPDF.Pdf.Extension;

public static class Format
{
    public static readonly char[][] EscapeChars = [['('], [')'], ['\\']];

    public static string ToEscapeString(this string s, Encoding encoding) => s.All(char.IsAscii) ? $"({s.ReplaceBeforeInsert(EscapeChars, ['\\']).ToStringByChars()})" : s.ToHexString(encoding);

    public static string ToHexString(this string s, Encoding encoding) => $"<{Convert.ToHexStringLower(encoding.GetBytes(s))}>";

    public static DeviceRGB ToDeviceRGB(this Color color) => new((double)color.R / 255, (double)color.G / 255, (double)color.B / 255);

    public static string ToPointString(this (IPoint X, IPoint Y) point, int height, string format) => ToPointString(point, (double)height, format);
    public static string ToPointString(this (IPoint X, IPoint Y) point, double height, string format) => $"{point.X.ToPointString(format)} {(height - point.Y.ToPoint()).ToPointString(format)}";

    public static string ToPointString(this IPoint point, string format) => ToPointString(point.ToPoint(), format);
    public static string ToPointString(this float point, string format) => ToPointString((double)point, format);
    public static string ToPointString(this double point, string format) =>
        format == "F%" ? point.ToString("F7", CultureInfo.InvariantCulture).TrimEnd('0').TrimEnd('.') :
            point <= long.MaxValue &&
            point >= long.MinValue &&
            point % 1d == 0d ? ((long)point).ToString() : point.ToString(format, CultureInfo.InvariantCulture);
}
