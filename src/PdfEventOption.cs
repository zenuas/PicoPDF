using Binder.Data;
using Mina.Extension;
using Pdf.Documents;
using Pdf.Documents.Security;
using Pdf.Font;
using Pdf.Operation;
using PicoPDF.Model;
using PicoPDF.Model.Elements;
using System;

namespace PicoPDF;

public class PdfEventOption
{
    public Func<IFontRegister> CreateFontRegister { get; init; } = () => new FontRegister().Return(x => x.RegisterDirectory([.. FontRegister.GetFontDirectories()]));
    public Func<IStandardEncryption?> CreateStandardEncryption { get; init; } = () => null;
    public Func<IMetadata?> CreateMetadata { get; init; } = () => null;
    public Func<SectionModel, SectionModel> BindSection { get; init; } = (section) => section;
    public Func<ISection, IElement, object?, IModelElement, IModelElement> BindElement { get; init; } = (_, _, _, model) => model;
    public Func<Page, IModelElement, int, int, IOperation> Mapping { get; init; } = ModelMapping.Mapping;
}
