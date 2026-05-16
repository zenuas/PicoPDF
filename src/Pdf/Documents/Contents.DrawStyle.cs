using Mina.Extension;
using PicoPDF.Loader.Elements;
using PicoPDF.Pdf.Color;
using PicoPDF.Pdf.Operation;
using System.Collections.Generic;

namespace PicoPDF.Pdf.Documents;

public partial class Contents
{
    public void DrawTextStyle(TextStyle style, double top, double left, double basey, double width, double height, IColor? color = null) => CreateDrawTextStyleOperations(style, top, left, basey, width, height, color).Each(Operations.Add);

    public static IEnumerable<IOperation> CreateDrawTextStyleOperations(TextStyle style, double top, double left, double basey, double width, double height, IColor? color = null)
    {
        var right = left + width;
        var linewidth = height / 20;

        if (style.HasFlag(TextStyle.Underline))
        {
            yield return CreateDrawLinesOperation([(left, basey + (linewidth * 2)), (right, basey + (linewidth * 2))], color, linewidth * 2);
        }
        if (style.HasFlag(TextStyle.DoubleUnderline))
        {
            yield return CreateDrawLinesOperation([(left, basey + linewidth), (right, basey + linewidth)], color, linewidth);
            yield return CreateDrawLinesOperation([(left, basey + (linewidth * 3)), (right, basey + (linewidth * 3))], color, linewidth);
        }
        if ((style & (TextStyle.Strikethrough | TextStyle.DoubleStrikethrough)) > 0)
        {
            var center = top + (height / 2);
            if (style.HasFlag(TextStyle.Strikethrough)) yield return CreateDrawLinesOperation([(left, center), (right, center)], color, linewidth);
            if (style.HasFlag(TextStyle.DoubleStrikethrough))
            {
                yield return CreateDrawLinesOperation([(left, center + linewidth), (right, center + linewidth)], color, linewidth);
                yield return CreateDrawLinesOperation([(left, center - linewidth), (right, center - linewidth)], color, linewidth);
            }
        }
    }

    public void DrawBorderStyle(TextStyle style, double top, double left, double width, double height, double? linewidth = null, IColor? color = null) => CreateDrawBorderStyleOperations(style, top, left, width, height, linewidth, color).Each(Operations.Add);

    public static IEnumerable<IOperation> CreateDrawBorderStyleOperations(TextStyle style, double top, double left, double width, double height, double? linewidth = null, IColor? color = null)
    {
        var bottom = top + height;
        var right = left + width;
        var line = linewidth ?? height / 20;

        if (style.HasFlag(TextStyle.Border))
        {
            yield return CreateDrawRectangleOperation(left, top, width, height, color, line);
        }
        if ((style & (TextStyle.BorderTop | TextStyle.BorderBottom)) == 0)
        {
            if (style.HasFlag(TextStyle.BorderLeft)) yield return CreateDrawLinesOperation([(left, top), (left, bottom)], color, line);
            if (style.HasFlag(TextStyle.BorderRight)) yield return CreateDrawLinesOperation([(right, top), (right, bottom)], color, line);
        }
        else if ((style & (TextStyle.BorderLeft | TextStyle.BorderRight)) == 0)
        {
            if (style.HasFlag(TextStyle.BorderTop)) yield return CreateDrawLinesOperation([(left, top), (right, top)], color, line);
            if (style.HasFlag(TextStyle.BorderBottom)) yield return CreateDrawLinesOperation([(left, bottom), (right, bottom)], color, line);
        }
        else
        {
            // draw one stroke
            switch (style & TextStyle.BorderStyleMask)
            {
                case TextStyle.BorderTop | TextStyle.BorderRight: yield return CreateDrawLinesOperation([(left, top), (right, top), (right, bottom)], color, line); break;
                case TextStyle.BorderRight | TextStyle.BorderBottom: yield return CreateDrawLinesOperation([(right, top), (right, bottom), (left, bottom)], color, line); break;
                case TextStyle.BorderBottom | TextStyle.BorderLeft: yield return CreateDrawLinesOperation([(right, bottom), (left, bottom), (left, top)], color, line); break;
                case TextStyle.BorderLeft | TextStyle.BorderTop: yield return CreateDrawLinesOperation([(left, bottom), (left, top), (right, top)], color, line); break;

                case TextStyle.BorderTop | TextStyle.BorderRight | TextStyle.BorderBottom: yield return CreateDrawLinesOperation([(left, top), (right, top), (right, bottom), (left, bottom)], color, line); break;
                case TextStyle.BorderRight | TextStyle.BorderBottom | TextStyle.BorderLeft: yield return CreateDrawLinesOperation([(right, top), (right, bottom), (left, bottom), (left, top)], color, line); break;
                case TextStyle.BorderBottom | TextStyle.BorderLeft | TextStyle.BorderTop: yield return CreateDrawLinesOperation([(right, bottom), (left, bottom), (left, top), (right, top)], color, line); break;
                case TextStyle.BorderLeft | TextStyle.BorderTop | TextStyle.BorderRight: yield return CreateDrawLinesOperation([(left, bottom), (left, top), (right, top), (right, bottom)], color, line); break;
            }
        }
    }
}
