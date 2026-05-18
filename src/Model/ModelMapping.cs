using Mina.Extension;
using PicoPDF.Model.Elements;
using PicoPDF.Pdf;
using PicoPDF.Pdf.Documents;
using System.Linq;

namespace PicoPDF.Model;

public static class ModelMapping
{
    public static void Mapping(Document doc, PageModel[] pages, PdfEventOption option)
    {
        foreach (var page in pages)
        {
            var pdfpage = doc.NewPage(page.Width, page.Height);
            page.Models.Each(section => section.Elements.Each(x => option.Mapping(pdfpage, x, section.Top, section.Left)));
        }
    }

    public static void Mapping(Page page, IModelElement model, int top, int left)
    {
        double posx = model.X + left;
        double posy = model.Y + top;
        switch (model)
        {
            case ITextModel x:
                _ = page.Contents.DrawText(x.Text, posy, posx, x.Size, [.. x.Font.Select(x => page.Document.GetFont(x.Path, x.Embed))], x.Width, x.Height, x.Style, x.Alignment, x.Color?.ToDeviceRGB());
                return;

            case ILineModel x:
                page.Contents.DrawLine(posx, posy, posx + x.Width, posy + x.Height, x.Color?.ToDeviceRGB(), x.LineWidth);
                return;

            case IRectangleModel x:
                page.Contents.DrawRectangle(posx, posy, x.Width, x.Height, x.Color?.ToDeviceRGB(), x.LineWidth);
                return;

            case IFillRectangleModel x:
                page.Contents.DrawFillRectangle(posx, posy, x.Width, x.Height, x.LineColor.ToDeviceRGB(), x.FillColor.ToDeviceRGB(), x.LineWidth);
                return;

            case ImageModel x:
                page.Contents.DrawImage(posx, posy, page.Document.GetImage(x), x.ZoomWidth, x.ZoomHeight);
                return;
        }
        throw new();
    }
}
