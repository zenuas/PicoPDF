using Binder.Data;
using PicoPDF.Model.Elements;
using System;

namespace PicoPDF.Pdf;

public class PdfEventOption
{
    public Func<ISection, IElement, IModelElement, IModelElement> BindElement { get; init; } = (_, _, model) => model;
}
