using System;
using System.Collections.Generic;
using System.Numerics;

namespace OpenType.Tables.PostScript;

public class SubroutineFrame
{
    public Action<Vector2[]> AddLine { get; init; } = _ => { };
    public Dictionary<int, float> TransientArray { get; init; } = [];
    public Vector2 CurrentPoint { get; set; }
    public int StemPairCount { get; set; } = 0;
    public bool IsEndchar { get; set; } = false;
    public int? Width { get; set; } = null;
}
