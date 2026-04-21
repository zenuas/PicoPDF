using System;
using System.Collections.Generic;
using System.Numerics;

namespace OpenType.Tables.PostScript;

public class SubroutineFrame
{
    public required byte[][]? LocalSubroutine { get; init; }
    public required byte[][] GlobalSubroutine { get; init; }
    public Action<Vector2[]> AddLine { get; init; } = _ => { };
    public Dictionary<int, float> TransientArray { get; init; } = [];
    public Vector2 CurrentPoint { get; set; }
    public Vector2? StartPoint { get; set; } = null;
    public int StemPairCount { get; set; } = 0;
    public bool IsEndchar { get; set; } = false;
    public int? Width { get; set; } = null;
}
