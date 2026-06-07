using Mina.Extension;
using Pdf.Documents.Security;
using Pdf.Drawing;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Pdf.Extension;

public static class Format
{
    public static readonly char[][] EscapeChars = [['('], [')'], ['\\']];

    public static string ToEscapeString(this string self, Encoding encoding) => self.All(char.IsAscii) ? $"({self.ReplaceBeforeInsert(EscapeChars, ['\\']).ToStringByChars()})" : self.ToHexString(encoding);
    public static string ToEncryptString(this string self, Encoding encoding, IConverter converter) => converter.Convert(encoding.GetBytes(self)).ToHexString();

    public static string ToHexString(this string self, Encoding encoding) => ToHexString(encoding.GetBytes(self));
    public static string ToHexString(this byte[] self) => $"<{Convert.ToHexStringLower(self)}>";

    public static DeviceRGB ToDeviceRGB(this Color self) => new((double)self.R / 255, (double)self.G / 255, (double)self.B / 255);

    public static string ToPointString(this (IPoint X, IPoint Y) self, int height, string format) => ToPointString(self, (double)height, format);
    public static string ToPointString(this (IPoint X, IPoint Y) self, double height, string format) => $"{self.X.ToPointString(format)} {(height - self.Y.ToPoint()).ToPointString(format)}";

    public static string ToPointString(this IEnumerable<IPoint> self, string format) => self.Select(x => x.ToPointString(format)).Join(" ");
    public static string ToPointString(this IEnumerable<float> self, string format) => self.Select(x => x.ToPointString(format)).Join(" ");
    public static string ToPointString(this IEnumerable<double> self, string format) => self.Select(x => x.ToPointString(format)).Join(" ");
    public static string ToPointString(this IPoint self, string format) => ToPointString(self.ToPoint(), format);
    public static string ToPointString(this float self, string format) => ToPointString((double)self, format);
    public static string ToPointString(this double self, string format) =>
        format == "F%" ? self.ToString("F7", CultureInfo.InvariantCulture).TrimEnd('0').TrimEnd('.') :
            self <= long.MaxValue &&
            self >= long.MinValue &&
            self % 1d == 0d ? ((long)self).ToString() : self.ToString(format, CultureInfo.InvariantCulture);
}
