namespace PicoPDF.Pdf;

public class PdfExportOption
{
    public bool Debug { get; init; } = false;
    public bool AppendCIDToUnicode { get; init; } = true;
    public bool ContentsStreamDeflate { get; init; } = true;
    public bool JpegStreamDeflate { get; init; } = true;
    public bool ImageStreamDeflate { get; init; } = true;
    public bool CMapStreamDeflate { get; init; } = true;
    public bool FontStreamDeflate { get; init; } = true;
    public bool OutputCrossReferenceTable { get; init; } = true;
    public string PointFormat { get; init; } = "F%";
}
