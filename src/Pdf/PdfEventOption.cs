using Binder.Data;
using PicoPDF.Loader.Sections;
using PicoPDF.Model;
using PicoPDF.Model.Elements;
using PicoPDF.Pdf.Documents;
using PicoPDF.Pdf.Font;
using PicoPDF.Pdf.XObject.Image;
using System;

namespace PicoPDF.Pdf;

public class PdfEventOption
{
    public Func<ISection, IElement, object?, IModelElement, IModelElement> BindElement { get; init; } = (_, _, _, model) => model;
    public Action<Page, Func<string, FontEmbed, Type0Font>, Func<ImageModel, IImageXObject>, IModelElement, int, int> Mapping { get; init; } = ModelMapping.Mapping;
}
