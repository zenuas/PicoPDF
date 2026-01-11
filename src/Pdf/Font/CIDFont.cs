using Mina.Extension;
using PicoPDF.Pdf.Element;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PicoPDF.Pdf.Font;

public class CIDFont : PdfObject, IFont
{
    public required string Name { get; init; }
    public required string BaseFont { get; init; }
    public required string Encoding { get; init; }
    public required CIDFontDictionary FontDictionary { get; init; }
    public required Encoding TextEncoding { get; init; }
    public Dictionary<char, int> Widths { get; init; } = new()
    {
        {' ', 500},  // SP
        {'!', 500},  // !
        {'"', 500},  // "
        {'#', 500},  // #
        {'$', 500},  // $
        {'%', 500},  // %
        {'&', 500},  // &
        {'\'', 500}, // '
        {'(', 500},  // (
        {')', 500},  // )
        {'*', 500},  // *
        {'+', 500},  // +
        {',', 500},  // ,
        {'-', 500},  // -
        {'.', 500},  // .
        {'/', 500},  // /
        {'0', 500},  // 0
        {'1', 500},  // 1
        {'2', 500},  // 2
        {'3', 500},  // 3
        {'4', 500},  // 4
        {'5', 500},  // 5
        {'6', 500},  // 6
        {'7', 500},  // 7
        {'8', 500},  // 8
        {'9', 500},  // 9
        {':', 500},  // :
        {';', 500},  // ;
        {'<', 500},  // <
        {'=', 500},  // =
        {'>', 500},  // >
        {'?', 500},  // ?
        {'@', 500},  // @
        {'A', 500},  // A
        {'B', 500},  // B
        {'C', 500},  // C
        {'D', 500},  // D
        {'E', 500},  // E
        {'F', 500},  // F
        {'G', 500},  // G
        {'H', 500},  // H
        {'I', 500},  // I
        {'J', 500},  // J
        {'K', 500},  // K
        {'L', 500},  // L
        {'M', 500},  // M
        {'N', 500},  // N
        {'O', 500},  // O
        {'P', 500},  // P
        {'Q', 500},  // Q
        {'R', 500},  // R
        {'S', 500},  // S
        {'T', 500},  // T
        {'U', 500},  // U
        {'V', 500},  // V
        {'W', 500},  // W
        {'X', 500},  // X
        {'Y', 500},  // Y
        {'Z', 500},  // Z
        {'[', 500},  // [
        {'\\', 500}, // \
        {']', 500},  // ]
        {'^', 500},  // ^
        {'_', 500},  // _
        {'`', 500},  // `
        {'a', 500},  // a
        {'b', 500},  // b
        {'c', 500},  // c
        {'d', 500},  // d
        {'e', 500},  // e
        {'f', 500},  // f
        {'g', 500},  // g
        {'h', 500},  // h
        {'i', 500},  // i
        {'j', 500},  // j
        {'k', 500},  // k
        {'l', 500},  // l
        {'m', 500},  // m
        {'n', 500},  // n
        {'o', 500},  // o
        {'p', 500},  // p
        {'q', 500},  // q
        {'r', 500},  // r
        {'s', 500},  // s
        {'t', 500},  // t
        {'u', 500},  // u
        {'v', 500},  // v
        {'w', 500},  // w
        {'x', 500},  // x
        {'y', 500},  // y
        {'z', 500},  // z
        {'{', 500},  // {
        {'|', 500},  // |
        {'}', 500},  // }
        {'~', 500},  // ~
    };

    public override void DoExport(PdfExportOption option)
    {
        RelatedObjects.Add(FontDictionary);
        _ = Elements.TryAdd("Type", $"/Font %{Name}");
        _ = Elements.TryAdd("Subtype", $"/Type0");
        _ = Elements.TryAdd("BaseFont", $"/{BaseFont}");
        _ = Elements.TryAdd("Encoding", $"/{Encoding}");
        _ = Elements.TryAdd("DescendantFonts", new ElementIndirectArray(FontDictionary));
    }

    public IEnumerable<byte> CreateTextShowingOperator(string s)
    {
        return SplitWidth(s, Widths)
            .Select(x => PdfUtility.ToStringEscapeBytes(s, TextEncoding).Concat(System.Text.Encoding.ASCII.GetBytes($"{x.Width} ")))
            .Flatten()
            .Prepend((byte)'[')
            .Concat(System.Text.Encoding.ASCII.GetBytes("] TJ"));
    }

    public static IEnumerable<(string Text, int Width)> SplitWidth(string text, Dictionary<char, int> widths)
    {
        foreach (var (values, found, separator) in text.SplitFor(widths.ContainsKey))
        {
            if (!values.IsEmpty()) yield return (values.ToStringByChars(), 0);
            if (found) yield return (separator.ToString(), widths[separator]);
        }
    }
}
