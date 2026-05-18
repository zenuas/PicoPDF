using Binder.Data;
using Mina.Extension;
using PicoPDF.Model;
using PicoPDF.Model.Elements;
using PicoPDF.Pdf.Documents;
using PicoPDF.Pdf.Font;
using System;

namespace PicoPDF.Pdf;

public class PdfEventOption
{
    public Func<IFontRegister> CreateFontRegister { get; init; } = () => new FontRegister().Return(x => x.RegisterDirectory([.. FontRegister.GetFontDirectories()]));
    public Func<ISection, IElement, object?, IModelElement, IModelElement> BindElement { get; init; } = (_, _, _, model) => model;
    public Action<Page, IModelElement, int, int> Mapping { get; init; } = ModelMapping.Mapping;
}
