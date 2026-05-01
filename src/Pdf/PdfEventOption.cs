using Binder.Data;
using PicoPDF.Model.Elements;
using System;

namespace PicoPDF.Pdf;

public class PdfEventOption
{
    public Func<ISection, IElement, object?, IModelElement, IModelElement> BindElement { get; init; } = (_, _, _, model) => model;
}
