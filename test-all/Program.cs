using Mina.Command;
using PicoPDF.TestAll;

var (command, xargs) = CommandLine.Run<ICommand>([
        ("", typeof(PdfCreate)),
        ("create", typeof(PdfCreate)),
        ("manual", typeof(ManualCreate)),
        ("deflate", typeof(Deflate)),
        ("cmap", typeof(CMapList)),
        ("name", typeof(NameRecordList)),
        ("svg", typeof(SvgOutput)),
        ("font-html", typeof(FontHtml)),
        ("font-list", typeof(FontList)),
        ("font-dump", typeof(FontDump)),
        ("font-export", typeof(FontExport)),
    ], args);

command.Run(xargs);

