namespace PicoPDF.Pdf;

public class PdfExportOption
{
    public bool ContentsStreamDeflate { get; set; } = true;
    public bool JpegStreamDeflate { get; set; } = true;
    public bool ImageStreamDeflate { get; set; } = true;
}
