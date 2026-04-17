using OpenType.Outline;
using System.Collections.Generic;
using System.Numerics;

namespace OpenType.Tables.PostScript;

public class SubroutineFrame
{
    public required byte[][]? LocalSubroutine { get; init; }
    public required byte[][] GlobalSubroutine { get; init; }
    public List<IEdge> Edges { get; init; } = [];
    public Dictionary<int, float> TransientArray { get; init; } = [];
    public Vector2 CurrentPoint { get; set; }
    public Vector2? StartPoint { get; set; } = null;
}
