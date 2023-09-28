namespace PicoPDF.Pdf.XObject;

public interface IImageXObject
{
    public string Name { get; init; }
    public int Width { get; init; }
    public int Height { get; init; }
}
