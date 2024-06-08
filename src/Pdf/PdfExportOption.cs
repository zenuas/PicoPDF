namespace PicoPDF.Pdf;

public class PdfExportOption
{
    public bool Debug { get; set; } = false;
    public bool AppendCIDToUnicode { get; set; } = true;
    public bool EmbeddedFont { get; set; } = true;
    public bool ContentsStreamDeflate { get; set; } = true;
    public bool JpegStreamDeflate { get; set; } = true;
    public bool ImageStreamDeflate { get; set; } = true;
    public bool CMapStreamDeflate { get; set; } = true;
}
