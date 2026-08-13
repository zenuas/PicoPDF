namespace OpenType;

public class LoadOption
{
    public Platforms[] PlatformIDOrder { get; init; } = [Platforms.Unicode];

    public bool UseVertical { get; init; } = false;
    public bool UseProportional { get; init; } = false;
}
