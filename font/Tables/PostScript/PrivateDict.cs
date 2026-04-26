using Mina.Extension;
using System.Collections.Generic;
using System.IO;

namespace OpenType.Tables.PostScript;

public class PrivateDict
{
    public required Dictionary<PrivateDictOperators, IntOrDouble[]> Dict { get; init; }
    public byte[][] LocalSubroutines { get; init; } = [];

    public IntOrDouble[]? BlueValues { get => Dict.TryGetValue(PrivateDictOperators.BlueValues, out var xs) ? xs : null; }
    public IntOrDouble[]? OtherBlues { get => Dict.TryGetValue(PrivateDictOperators.OtherBlues, out var xs) ? xs : null; }
    public IntOrDouble[]? FamilyBlues { get => Dict.TryGetValue(PrivateDictOperators.FamilyBlues, out var xs) ? xs : null; }
    public IntOrDouble[]? FamilyOtherBlues { get => Dict.TryGetValue(PrivateDictOperators.FamilyOtherBlues, out var xs) ? xs : null; }
    public IntOrDouble BlueScale { get => Dict.TryGetValue(PrivateDictOperators.BlueScale, out var xs) ? xs[0] : 0.039625; }
    public IntOrDouble BlueShift { get => Dict.TryGetValue(PrivateDictOperators.BlueShift, out var xs) ? xs[0] : 7; }
    public IntOrDouble BlueFuzz { get => Dict.TryGetValue(PrivateDictOperators.BlueFuzz, out var xs) ? xs[0] : 1; }
    public IntOrDouble? StdHW { get => Dict.TryGetValue(PrivateDictOperators.StdHW, out var xs) ? xs[0] : null; }
    public IntOrDouble? StdVW { get => Dict.TryGetValue(PrivateDictOperators.StdVW, out var xs) ? xs[0] : null; }
    public IntOrDouble[]? StemSnapH { get => Dict.TryGetValue(PrivateDictOperators.StemSnapH, out var xs) ? xs : null; }
    public IntOrDouble[]? StemSnapV { get => Dict.TryGetValue(PrivateDictOperators.StemSnapV, out var xs) ? xs : null; }
    public bool ForceBold { get => Dict.TryGetValue(PrivateDictOperators.ForceBold, out var xs) && xs[0].ToInt() != 0; }
    public IntOrDouble LanguageGroup { get => Dict.TryGetValue(PrivateDictOperators.LanguageGroup, out var xs) ? xs[0] : 0; }
    public IntOrDouble ExpansionFactor { get => Dict.TryGetValue(PrivateDictOperators.ExpansionFactor, out var xs) ? xs[0] : 0.06; }
    public IntOrDouble InitialRandomSeed { get => Dict.TryGetValue(PrivateDictOperators.InitialRandomSeed, out var xs) ? xs[0] : 0; }
    public int? SubrsOffset { get => Dict.TryGetValue(PrivateDictOperators.Subrs, out var xs) ? xs[0].ToInt() : null; }
    public int DefaultWidthX { get => Dict.TryGetValue(PrivateDictOperators.DefaultWidthX, out var xs) ? xs[0].ToInt() : 0; }
    public int NominalWidthX { get => Dict.TryGetValue(PrivateDictOperators.NominalWidthX, out var xs) ? xs[0].ToInt() : 0; }

    public static PrivateDict ReadFrom(byte[] bytes, Stream stream, long offset)
    {
        var dict = new PrivateDict() { Dict = TopDict.BytesToDict<PrivateDictOperators>(bytes) };
        var subr = dict.SubrsOffset is { } subr_offset ? CompactFontFormat.ReadIndexData(stream.SeekTo(offset + subr_offset)) : [];

        return new()
        {
            Dict = dict.Dict,
            LocalSubroutines = subr,
        };
    }

    public void WriteWithoutDictAndOffsetUpdate(Stream stream, long offset)
    {
        if (Dict.ContainsKey(PrivateDictOperators.Subrs))
        {
            Dict[PrivateDictOperators.Subrs] = [stream.Position - offset];
            CompactFontFormat.WriteIndexData(stream, LocalSubroutines);
        }
    }
}
