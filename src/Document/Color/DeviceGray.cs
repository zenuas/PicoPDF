namespace PicoPDF.Document.Color;

public class DeviceGray : IColor
{
    public required double Gray { get; init; }

    public string CreateColor(bool isstroke) => $"{Gray} {(isstroke ? "G" : "g")}";
}
