namespace OpenType;

public class LoadOption
{
    public Platforms[] PlatformIDOrder { get; init; } = [Platforms.Unicode];

    public bool ForceEmbedded { get; init; } = false;
    public bool SupportVertical { get; init; } = false;
}
