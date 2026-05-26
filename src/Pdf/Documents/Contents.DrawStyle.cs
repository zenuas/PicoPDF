using PicoPDF.Loader.Elements;
using PicoPDF.Pdf.Drawing;
using PicoPDF.Pdf.Operation;
using System.Collections.Generic;

namespace PicoPDF.Pdf.Documents;

public partial class Contents
{
    public static IEnumerable<IOperation> CreateDrawTextStyleOperations(TextStyles style, double top, double left, double basey, double width, double height, IColor? color = null)
    {
        var right = left + width;
        var linewidth = height / 20;

        if (style.HasFlag(TextStyles.Underline))
        {
            yield return CreateDrawLinesOperation([(left, basey + (linewidth * 2)), (right, basey + (linewidth * 2))], color, linewidth * 2);
        }
        if (style.HasFlag(TextStyles.DoubleUnderline))
        {
            yield return CreateDrawLinesOperation([(left, basey + linewidth), (right, basey + linewidth)], color, linewidth);
            yield return CreateDrawLinesOperation([(left, basey + (linewidth * 3)), (right, basey + (linewidth * 3))], color, linewidth);
        }
        if ((style & (TextStyles.Strikethrough | TextStyles.DoubleStrikethrough)) > 0)
        {
            var center = top + (height / 2);
            if (style.HasFlag(TextStyles.Strikethrough)) yield return CreateDrawLinesOperation([(left, center), (right, center)], color, linewidth);
            if (style.HasFlag(TextStyles.DoubleStrikethrough))
            {
                yield return CreateDrawLinesOperation([(left, center + linewidth), (right, center + linewidth)], color, linewidth);
                yield return CreateDrawLinesOperation([(left, center - linewidth), (right, center - linewidth)], color, linewidth);
            }
        }
    }

    public static IEnumerable<IOperation> CreateDrawBorderStyleOperations(TextStyles style, double top, double left, double width, double height, double? linewidth = null, IColor? color = null)
    {
        var bottom = top + height;
        var right = left + width;
        var line = linewidth ?? (height / 20);

        if (style.HasFlag(TextStyles.Border))
        {
            yield return CreateDrawRectangleOperation(left, top, width, height, color, line);
        }
        if ((style & (TextStyles.BorderTop | TextStyles.BorderBottom)) == 0)
        {
            if (style.HasFlag(TextStyles.BorderLeft)) yield return CreateDrawLinesOperation([(left, top), (left, bottom)], color, line);
            if (style.HasFlag(TextStyles.BorderRight)) yield return CreateDrawLinesOperation([(right, top), (right, bottom)], color, line);
        }
        else if ((style & (TextStyles.BorderLeft | TextStyles.BorderRight)) == 0)
        {
            if (style.HasFlag(TextStyles.BorderTop)) yield return CreateDrawLinesOperation([(left, top), (right, top)], color, line);
            if (style.HasFlag(TextStyles.BorderBottom)) yield return CreateDrawLinesOperation([(left, bottom), (right, bottom)], color, line);
        }
        else
        {
            // draw one stroke
            switch (style & TextStyles.BorderStyleMask)
            {
                case TextStyles.BorderTop | TextStyles.BorderRight: yield return CreateDrawLinesOperation([(left, top), (right, top), (right, bottom)], color, line); break;
                case TextStyles.BorderRight | TextStyles.BorderBottom: yield return CreateDrawLinesOperation([(right, top), (right, bottom), (left, bottom)], color, line); break;
                case TextStyles.BorderBottom | TextStyles.BorderLeft: yield return CreateDrawLinesOperation([(right, bottom), (left, bottom), (left, top)], color, line); break;
                case TextStyles.BorderLeft | TextStyles.BorderTop: yield return CreateDrawLinesOperation([(left, bottom), (left, top), (right, top)], color, line); break;

                case TextStyles.BorderTop | TextStyles.BorderRight | TextStyles.BorderBottom: yield return CreateDrawLinesOperation([(left, top), (right, top), (right, bottom), (left, bottom)], color, line); break;
                case TextStyles.BorderRight | TextStyles.BorderBottom | TextStyles.BorderLeft: yield return CreateDrawLinesOperation([(right, top), (right, bottom), (left, bottom), (left, top)], color, line); break;
                case TextStyles.BorderBottom | TextStyles.BorderLeft | TextStyles.BorderTop: yield return CreateDrawLinesOperation([(right, bottom), (left, bottom), (left, top), (right, top)], color, line); break;
                case TextStyles.BorderLeft | TextStyles.BorderTop | TextStyles.BorderRight: yield return CreateDrawLinesOperation([(left, bottom), (left, top), (right, top), (right, bottom)], color, line); break;
            }
        }
    }
}
