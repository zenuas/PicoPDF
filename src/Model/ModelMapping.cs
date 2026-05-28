using Binder.Data;
using Mina.Extension;
using PicoPDF.Loader.Sections;
using PicoPDF.Model.Elements;
using PicoPDF.Pdf;
using PicoPDF.Pdf.Documents;
using PicoPDF.Pdf.Drawing;
using PicoPDF.Pdf.Extension;
using PicoPDF.Pdf.Operation;
using System.Linq;

namespace PicoPDF.Model;

public static class ModelMapping
{
    public static void Mapping(Document document, PageModel[] pages, PdfEventOption option)
    {
        foreach (var page in pages)
        {
            var pdfpage = document.NewPage(page.Width, page.Height);
            page.Models
                .Select(section =>
                {
                    if ((section.Section as ISectionStyle)?.Style.HasFlag(SectionStyles.Clipping) ?? false)
                    {
                        var without_cross_sections = section.Elements
                            .Where(x => x.Element is not ICrossSectionElement)
                            .Select(x => option.Mapping(pdfpage, x, section.Top, section.Left));

                        var cross_sections = section.Elements
                            .Where(x => x.Element is ICrossSectionElement)
                            .Select(x => option.Mapping(pdfpage, x, section.Top, section.Left));

                        return [new DrawClipping()
                            {
                                X = new PointValue(section.Left),
                                Y = new PointValue(section.Top),
                                Width = new PointValue(section.Width),
                                Height = new PointValue(section.Height),
                                Operations = [.. without_cross_sections]
                            }, ..cross_sections];
                    }
                    else
                    {
                        return section.Elements.Select(x => option.Mapping(pdfpage, x, section.Top, section.Left));
                    }
                })
                .Flatten()
                .Each(pdfpage.Contents.Operations.Add);
        }
    }

    public static IOperation Mapping(Page page, IModelElement model, int top, int left)
    {
        double posx = model.X + left;
        double posy = model.Y + top;
        switch (model)
        {
            case ITextModel x:
                return Contents.CreateDrawText(page.Document, x.Text, posx, posy, x.Size, [.. x.Font.Select(x => page.Document.GetFont(x.Path, x.Embed))], x.Width, x.Height, x.Style, x.Alignment, x.Color?.ToDeviceRGB());

            case ILineModel x:
                return Contents.CreateDrawLinesOperation([(posx, posy), (posx + x.Width, posy + x.Height)], x.Color?.ToDeviceRGB(), x.LineWidth);

            case IRectangleModel x:
                return Contents.CreateDrawRectangleOperation(posx, posy, x.Width, x.Height, x.Color?.ToDeviceRGB(), x.LineWidth);

            case IFillRectangleModel x:
                return Contents.CreateDrawFillRectangleOperation(posx, posy, x.Width, x.Height, x.LineColor.ToDeviceRGB(), x.FillColor.ToDeviceRGB(), x.LineWidth);

            case ImageModel x:
                return Contents.CreateDrawImageOperation(posx, posy, page.Document.GetImage(x), x.ZoomWidth, x.ZoomHeight);
        }
        throw new();
    }
}
