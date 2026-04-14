using System.Collections.Generic;

namespace OpenType.Tables.PostScript;

public class SubroutineFrame
{
    public required byte[][]? LocalSubroutine { get; init; }
    public required byte[][] GlobalSubroutine { get; init; }
    public Dictionary<int, float> TransientArray { get; init; } = [];
}
