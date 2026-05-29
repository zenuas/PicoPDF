using Mina.Command;
using PicoPDF.TestAll;

var (command, xargs) = CommandLine.Run<ICommand>([
        ("", typeof(PdfCreate)),
        ("create", typeof(PdfCreate)),
        ("manual", typeof(ManualCreate)),
        ("encrypt", typeof(EncryptCreate)),
        ("manual-args", typeof(ManualArgsCreate)),
        ("deflate", typeof(Deflate)),
        ("cmap", typeof(CMapList)),
        ("name", typeof(NameRecordList)),
        ("svg", typeof(SvgOutput)),
        ("font-html", typeof(FontHtml)),
        ("fonts-all-html", typeof(FontsAllHtml)),
        ("font-list", typeof(FontList)),
        ("font-dump", typeof(FontDump)),
        ("font-export", typeof(FontExport)),
        ("cff-charstrings", typeof(CffCharStrings)),
        ("color-dump", typeof(ColorDump)),
    ], args);

command.Run(xargs);
