namespace PicoPDF.Pdf.XObject.Image;

public interface IImageXObject : IXObject
{
    public string Name { get; init; }
    public int Width { get; init; }
    public int Height { get; init; }
}
