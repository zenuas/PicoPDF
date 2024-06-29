namespace PicoPDF.OpenType;

public class LoadOption
{
    public Platforms[] PlatformIDOrder { get; init; } = [Platforms.Windows, Platforms.Unicode];
}
