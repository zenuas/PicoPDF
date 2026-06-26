using Binder;
using Pdf.Documents;
using PicoPDF.Loader;
using PicoPDF.Loader.Sections;
using PicoPDF.Model;
using System;
using System.Collections.Generic;
using System.Data;

namespace PicoPDF;

public static class PdfFactory
{
    public static Document CreateDocument<T>(string json, IEnumerable<T> datas, Dictionary<string, Func<T, object>>? mapper = null, PdfEventOption? option = null) => CreateDocument(json, (section) => SectionBinder.Bind<T, PageModel, SectionModel>(section, datas, mapper), option);
    public static Document CreateDocument(string json, DataTable table, PdfEventOption? option = null) => CreateDocument(json, (section) => SectionBinder.Bind<PageModel, SectionModel>(section, table), option);
    public static Document CreateDocument(string json, DataView view, PdfEventOption? option = null) => CreateDocument(json, (section) => SectionBinder.Bind<PageModel, SectionModel>(section, view), option);

    public static Document CreateDocument(string json, Func<PageSection, PageModel[]> pages, PdfEventOption? option = null)
    {
        var opt = option ?? new();
        var encrypt = opt.CreateStandardEncryption();
        var meta = opt.CreateMetadata();
        var document = new Document
        {
            FontRegister = opt.CreateFontRegister(),
            Encrypt = encrypt,
            StreamHandler = encrypt?.StreamHandler,
            StringHandler = encrypt?.StringHandler,
            EmbeddedFileStreamsHandler = encrypt?.EmbeddedFileStreamsHandler,
            Info = meta is TrailerInfo info ? info : null,
        };
        if (meta is XmpMetadata xmp)
        {
            document.Catalog.RelatedObjects.Add(xmp);
            document.Catalog.Elements.Add("Metadata", xmp);
        }
        ModelMapping.Mapping(document, pages(JsonLoader.CreatePageFromJsonFile(json, opt)), opt);
        return document;
    }
}
