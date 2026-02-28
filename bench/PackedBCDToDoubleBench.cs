using BenchmarkDotNet.Attributes;
using OpenType.Tables.PostScript;
using System;

namespace PicoPDF.Benchmark;

public class PackedBCDToDoubleBench
{
    [Benchmark]
    public void DoubleToMyParse()
    {
        MyPackedBCDToDouble([0x00, 0x0a, 0x01, 0x04, 0x00, 0x05, 0x04, 0x01, 0x0c, 0x03]);
    }

    [Benchmark]
    public void DoubleToStringParse()
    {
        DictData.PackedBCDToDouble([0x00, 0x0a, 0x01, 0x04, 0x00, 0x05, 0x04, 0x01, 0x0c, 0x03]);
    }

    public static double MyPackedBCDToDouble(byte[] bytes)
    {
        var value = 0d;
        var i = 0;
        var sign = true;

        if (i < bytes.Length && bytes[i] == 0x0e)
        {
            sign = false;
            i++;
        }

        for (; i < bytes.Length && bytes[i] <= 9; i++)
        {
            value = (value * 10) + bytes[i];
        }

        if (i < bytes.Length && bytes[i] == 0x0a)
        {
            i++;
            var dec = 0.1;
            for (; i < bytes.Length && bytes[i] <= 9; i++, dec /= 10)
            {
                value += bytes[i] * dec;
            }
        }

        if (i < bytes.Length && bytes[i] is 0x0b or 0x0c)
        {
            var esign = bytes[i] == 0x0b;
            i++;
            var e = 0;
            for (; i < bytes.Length && bytes[i] <= 9; i++)
            {
                e = (e * 10) + bytes[i];
            }
            value *= Math.Pow(10, esign ? e : -e);
        }

        return sign ? value : -value;
    }
}
