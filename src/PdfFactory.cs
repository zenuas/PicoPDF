using Binder;
using Pdf.Documents;
using Pdf.Documents.Security;
using PicoPDF.Loader;
using PicoPDF.Loader.Sections;
using PicoPDF.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace PicoPDF;

public static class PdfFactory
{
    public static Document Create<T>(string json, IEnumerable<T> datas, Dictionary<string, Func<T, object>>? mapper = null, PdfEventOption? option = null) => Create(json, (section) => SectionBinder.Bind<T, PageModel, SectionModel>(section, datas, mapper), option);
    public static Document Create(string json, DataTable table, PdfEventOption? option = null) => Create(json, (section) => SectionBinder.Bind<PageModel, SectionModel>(section, table), option);
    public static Document Create(string json, DataView view, PdfEventOption? option = null) => Create(json, (section) => SectionBinder.Bind<PageModel, SectionModel>(section, view), option);

    public static Document Create(string json, Func<PageSection, PageModel[]> pages, PdfEventOption? option = null)
    {
        var opt = option ?? new();
        var document = Create(opt);
        ModelMapping.Mapping(document, pages(Path.Exists(json) ? JsonLoader.CreatePageFromJsonFile(json, opt) : JsonLoader.CreatePageFromJson(json, opt)), opt);
        return document;
    }

    public static Document Create(PdfEventOption? option = null)
    {
        var opt = option ?? new();
        var encrypt = opt.CreateStandardEncryption();
        var meta = opt.CreateMetadata();
        var document = new Document
        {
            Version = opt.PDFVersion ?? (encrypt is StandardEncryption6 ? 20 : 17),
            FontRegister = opt.CreateFontRegister(),
            Encrypt = encrypt,
            Info = meta is TrailerInfo info ? info : null,
            DocumentID = encrypt is StandardEncryption4 stden4 ? (stden4.DocumentID, stden4.DocumentID) : null,
        };
        if (meta is XmpMetadata xmp)
        {
            document.Catalog.RelatedObjects.Add(xmp);
            document.Catalog.Elements.Add("Metadata", xmp);
        }
        return document;
    }
}
