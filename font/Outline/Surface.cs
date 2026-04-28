namespace OpenType.Outline;

public class Surface
{
    public required float XMin { get; init; }
    public required float YMin { get; init; }
    public required float XMax { get; init; }
    public required float YMax { get; init; }
    public required IEdge[] Edges { get; init; }
}
