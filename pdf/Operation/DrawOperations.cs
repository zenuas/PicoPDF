using Mina.Extension;
using Pdf.Documents;
using Pdf.Drawing;
using System.Collections.Generic;
using System.IO;

namespace Pdf.Operation;

public class DrawOperations : IOperation, IHaveOperations
{
    public required IPoint X { get; init; }
    public required IPoint Y { get; init; }
    public required IPoint Width { get; init; }
    public required IPoint Height { get; init; }
    public required IOperation[] Operations { get; init; }

    public void OperationWrite(int width, int height, Stream writer, PdfExportOption option)
    {
        Operations.Each(x => x.OperationWrite(width, height, writer, option));
    }

    public static IEnumerable<IOperation> CreateTextStyle(TextStyles style, double top, double left, double basey, double width, double height, IColor? color = null)
    {
        var right = left + width;
        var linewidth = height / 20;

        if (style.HasFlag(TextStyles.Underline))
        {
            yield return DrawLine.Create([(left, basey + (linewidth * 2)), (right, basey + (linewidth * 2))], color, linewidth * 2);
        }
        if (style.HasFlag(TextStyles.DoubleUnderline))
        {
            yield return DrawLine.Create([(left, basey + linewidth), (right, basey + linewidth)], color, linewidth);
            yield return DrawLine.Create([(left, basey + (linewidth * 3)), (right, basey + (linewidth * 3))], color, linewidth);
        }
        if ((style & (TextStyles.Strikethrough | TextStyles.DoubleStrikethrough)) > 0)
        {
            var center = top + (height / 2);
            if (style.HasFlag(TextStyles.Strikethrough)) yield return DrawLine.Create([(left, center), (right, center)], color, linewidth);
            if (style.HasFlag(TextStyles.DoubleStrikethrough))
            {
                yield return DrawLine.Create([(left, center + linewidth), (right, center + linewidth)], color, linewidth);
                yield return DrawLine.Create([(left, center - linewidth), (right, center - linewidth)], color, linewidth);
            }
        }
    }

    public static IEnumerable<IOperation> CreateBorderStyle(TextStyles style, double top, double left, double width, double height, double? linewidth = null, IColor? color = null)
    {
        var bottom = top + height;
        var right = left + width;
        var line = linewidth ?? (height / 20);

        if (style.HasFlag(TextStyles.Border))
        {
            yield return DrawRectangle.Create(left, top, width, height, color, line);
        }
        if ((style & (TextStyles.BorderTop | TextStyles.BorderBottom)) == 0)
        {
            if (style.HasFlag(TextStyles.BorderLeft)) yield return DrawLine.Create([(left, top), (left, bottom)], color, line);
            if (style.HasFlag(TextStyles.BorderRight)) yield return DrawLine.Create([(right, top), (right, bottom)], color, line);
        }
        else if ((style & (TextStyles.BorderLeft | TextStyles.BorderRight)) == 0)
        {
            if (style.HasFlag(TextStyles.BorderTop)) yield return DrawLine.Create([(left, top), (right, top)], color, line);
            if (style.HasFlag(TextStyles.BorderBottom)) yield return DrawLine.Create([(left, bottom), (right, bottom)], color, line);
        }
        else
        {
            // draw one stroke
            switch (style & TextStyles.BorderStyleMask)
            {
                case TextStyles.BorderTop | TextStyles.BorderRight: yield return DrawLine.Create([(left, top), (right, top), (right, bottom)], color, line); break;
                case TextStyles.BorderRight | TextStyles.BorderBottom: yield return DrawLine.Create([(right, top), (right, bottom), (left, bottom)], color, line); break;
                case TextStyles.BorderBottom | TextStyles.BorderLeft: yield return DrawLine.Create([(right, bottom), (left, bottom), (left, top)], color, line); break;
                case TextStyles.BorderLeft | TextStyles.BorderTop: yield return DrawLine.Create([(left, bottom), (left, top), (right, top)], color, line); break;

                case TextStyles.BorderTop | TextStyles.BorderRight | TextStyles.BorderBottom: yield return DrawLine.Create([(left, top), (right, top), (right, bottom), (left, bottom)], color, line); break;
                case TextStyles.BorderRight | TextStyles.BorderBottom | TextStyles.BorderLeft: yield return DrawLine.Create([(right, top), (right, bottom), (left, bottom), (left, top)], color, line); break;
                case TextStyles.BorderBottom | TextStyles.BorderLeft | TextStyles.BorderTop: yield return DrawLine.Create([(right, bottom), (left, bottom), (left, top), (right, top)], color, line); break;
                case TextStyles.BorderLeft | TextStyles.BorderTop | TextStyles.BorderRight: yield return DrawLine.Create([(left, bottom), (left, top), (right, top), (right, bottom)], color, line); break;
            }
        }
    }
}
