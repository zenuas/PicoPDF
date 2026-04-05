using Mina.Extension;

namespace PicoPDF.TestAll;

public class FontHtml : SvgOutput
{
    public override void Run(string[] args)
    {
        var fontreg = CreateFontRegister();
        var font = fontreg.LoadComplete(Font);
        var name = font.Name.NameRecords.FindFirstOrNullValue(x => x.NameRecord.NameID == 1)?.Name ?? font.PostScriptName;

        Output.WriteLine($"""
<!doctype html>
<head>
    <meta charset="utf-8">
    <title>Font Html | {name}</title>
    <style>
    body
    {@$"{{
        font-family: ""{name}"";
    }}"}
    </style>
</head>

<body>
<table border="1">
<thead>
    <tr>
        <th>GID</th>
        <th>Char</th>
        <th>Code</th>
        <th>Outline</th>
    </tr>
</thead>
<tbody>
""");
        for (var i = 0; i <= 0xFF_FFFF; i++)
        {
            var gid = font.CharToGID(i);
            if (gid == 0) continue;

            var s = char.ConvertFromUtf32(i);
            Output.WriteLine($"""
    <tr>
        <td>{gid}</td>
        <td>{s}</td>
        <td>U+{(i <= 0xFFFF ? i.ToString("x4") : i.ToString("x6"))}</td>
        <td>
""");
            OutputSvg(font, [i]);
            Output.WriteLine("""
        </td>
    </tr>
""");
        }
        Output.WriteLine("""
</tbody>
</table>
</body>
</html>
""");
        Output.Flush();
    }
}
