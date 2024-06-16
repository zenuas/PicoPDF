namespace PicoPDF.Pdf;

public class PdfExportOption
{
    public bool Debug { get; init; } = false;
    public bool AppendCIDToUnicode { get; init; } = true;
    public bool EmbeddedFont { get; init; } = false;
    public bool ContentsStreamDeflate { get; init; } = true;
    public bool JpegStreamDeflate { get; init; } = true;
    public bool ImageStreamDeflate { get; init; } = true;
    public bool CMapStreamDeflate { get; init; } = true;
}
