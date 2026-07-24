using System.Drawing;

namespace Image;

public readonly struct Color8 : IColor
{
    public byte R { get; init; }
    public byte G { get; init; }
    public byte B { get; init; }
    public byte A { get; init; }

    public static Color8 FromRgb(byte red, byte green, byte blue) => new() { R = red, G = green, B = blue, A = 255 };
    public static Color8 FromRgb(int red, int green, int blue) => new() { R = (byte)red, G = (byte)green, B = (byte)blue, A = 255 };
    public static Color8 FromArgb(byte alpha, byte red, byte green, byte blue) => new() { R = red, G = green, B = blue, A = alpha };
    public static Color8 FromArgb(int alpha, int red, int green, int blue) => new() { R = (byte)red, G = (byte)green, B = (byte)blue, A = (byte)alpha };

    public IColor AlphaBlend()
    {
        var a = A;
        if (a == 255) return this;

        var alpha = (255 * (255 - a)) + 127;
        var r = ((R * a) + alpha) / 255;
        var g = ((G * a) + alpha) / 255;
        var b = ((B * a) + alpha) / 255;
        return Color8.FromRgb(r, g, b);
    }

    public Color ToColor() => Color.FromArgb(A, R, G, B);

    public override string ToString() => $"Color8 [A=0x{A:X2}, R=0x{R:X2}, G=0x{G:X2}, B=0x{B:X2}]";
}
