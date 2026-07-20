using System.Drawing;

namespace Image;

public readonly struct Color16 : IColor
{
    public ushort R { get; init; }
    public ushort G { get; init; }
    public ushort B { get; init; }
    public ushort A { get; init; }

    public static Color16 FromRgb(ushort red, ushort green, ushort blue) => new() { R = red, G = green, B = blue, A = 65535 };
    public static Color16 FromRgb(int red, int green, int blue) => new() { R = (ushort)red, G = (ushort)green, B = (ushort)blue, A = 65535 };
    public static Color16 FromArgb(ushort alpha, ushort red, ushort green, ushort blue) => new() { R = red, G = green, B = blue, A = alpha };
    public static Color16 FromArgb(int alpha, int red, int green, int blue) => new() { R = (ushort)red, G = (ushort)green, B = (ushort)blue, A = (ushort)alpha };

    public IColor AlphaBlend()
    {
        var a = A;
        if (a == 65535) return this;

        var r = ((R * a) + (65535 * (65535 - a)) + 32767) / 65535;
        var g = ((G * a) + (65535 * (65535 - a)) + 32767) / 65535;
        var b = ((B * a) + (65535 * (65535 - a)) + 32767) / 65535;
        return Color16.FromRgb(r, g, b);
    }

    public Color ToColor() => Color.FromArgb(A / 256, R / 256, G / 256, B / 256);

    public override string ToString() => $"Color16 [A=0x{A:X4}, R=0x{R:X4}, G=0x{G:X4}, B=0x{B:X4}]";
}
