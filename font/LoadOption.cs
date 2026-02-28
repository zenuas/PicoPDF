namespace OpenType;

public class LoadOption
{
    public Platforms[] PlatformIDOrder { get; init; } = [Platforms.Windows, Platforms.Unicode];

    public bool ForceEmbedded { get; init; } = false;
}
