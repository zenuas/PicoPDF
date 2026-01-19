using Mina.Extension;
using PicoPDF.OpenType.Tables.PostScript;
using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace PicoPDF.Test;

public class CompactFontFormatTest
{
    public record IntBytes(int Value, byte[] Bytes);

    public static IntBytes DictDataNumber(int number)
    {
        var bytes = DictData.DictDataNumberToBytes(number);
        return new(DictData.ReadDictDataNumber(bytes[0], bytes[1..].AsSpan()), bytes);
    }

    [Fact]
    public void DictDataNumberTest()
    {
        Assert.Equal(DictDataNumber(0).Value, 0);

        // size 1
        Assert.Equivalent(DictDataNumber(-107), new IntBytes(-107, [32]));
        Assert.Equivalent(DictDataNumber(-106), new IntBytes(-106, [33]));
        Assert.Equivalent(DictDataNumber(106), new IntBytes(106, [245]));
        Assert.Equivalent(DictDataNumber(107), new IntBytes(107, [246]));
        for (var i = -106; i <= 107; i++)
        {
            Assert.Equal(DictDataNumber(i).Value, i);
        }

        // size 2 positive
        Assert.Equivalent(DictDataNumber(108), new IntBytes(108, [247, 0]));
        Assert.Equivalent(DictDataNumber(109), new IntBytes(109, [247, 1]));
        Assert.Equivalent(DictDataNumber(363), new IntBytes(363, [247, 255]));
        Assert.Equivalent(DictDataNumber(364), new IntBytes(364, [248, 0]));
        Assert.Equivalent(DictDataNumber(1130), new IntBytes(1130, [250, 254]));
        Assert.Equivalent(DictDataNumber(1131), new IntBytes(1131, [250, 255]));
        for (var i = 108; i <= 1131; i++)
        {
            Assert.Equal(DictDataNumber(i).Value, i);
        }

        // size 2 negative
        Assert.Equivalent(DictDataNumber(-108), new IntBytes(-108, [251, 0]));
        Assert.Equivalent(DictDataNumber(-109), new IntBytes(-109, [251, 1]));
        Assert.Equivalent(DictDataNumber(-1130), new IntBytes(-1130, [254, 254]));
        Assert.Equivalent(DictDataNumber(-1131), new IntBytes(-1131, [254, 255]));
        for (var i = -1131; i <= -108; i++)
        {
            Assert.Equal(DictDataNumber(i).Value, i);
        }

        // size 3
        Assert.Equivalent(DictDataNumber(-32768), new IntBytes(-32768, [28, 0x80, 0x00]));
        Assert.Equivalent(DictDataNumber(-32767), new IntBytes(-32767, [28, 0x80, 0x01]));
        Assert.Equivalent(DictDataNumber(32766), new IntBytes(32766, [28, 0x7F, 0xFE]));
        Assert.Equivalent(DictDataNumber(32767), new IntBytes(32767, [28, 0x7F, 0xFF]));

        // size 5
        Assert.Equivalent(DictDataNumber(-2147483648), new IntBytes(-2147483648, [29, 0x80, 0x00, 0x00, 0x00]));
        Assert.Equivalent(DictDataNumber(-2147483647), new IntBytes(-2147483647, [29, 0x80, 0x00, 0x00, 0x01]));
        Assert.Equivalent(DictDataNumber(2147483646), new IntBytes(2147483646, [29, 0x7F, 0xFF, 0xFF, 0xFE]));
        Assert.Equivalent(DictDataNumber(2147483647), new IntBytes(2147483647, [29, 0x7F, 0xFF, 0xFF, 0xFF]));
    }

    [Fact]
    public void DoubleToPackedBCDTest()
    {
        Assert.Equal(DictData.DoubleToPackedBCD(0), [0]);
        Assert.Equal(DictData.DoubleToPackedBCD(1.40541e-5), [0x01, 0x0a, 0x04, 0x00, 0x05, 0x04, 0x01, 0x0c, 0x00, 0x05]);
        Assert.Equal(DictData.DoubleToPackedBCD(1.40541e17), [0x01, 0x0a, 0x04, 0x00, 0x05, 0x04, 0x01, 0x0b, 0x01, 0x07]);
    }

    [Fact]
    public void PackedBCDToDoubleTest()
    {
        Assert.Equal(DictData.PackedBCDToDouble([0x00]), 0);
        Assert.Equal(DictData.PackedBCDToDouble([0x00, 0x0a, 0x01, 0x04, 0x00, 0x05, 0x04, 0x01, 0x0c, 0x03]), 0.140541e-3);
        Assert.Equal(DictData.PackedBCDToDouble([0x00, 0x0a, 0x01, 0x04, 0x00, 0x05, 0x04, 0x01, 0x0b, 0x03]), 0.140541e3);
    }

    public static IEnumerable<byte> MakeBytes(int count)
    {
        byte b = 0;
        for (var i = 0; i < count; i++)
        {
            yield return b++;
        }
    }

    public static void Write3Bytes(Stream stream, int x) => stream.Write([(byte)((x >> 16) & 0xFF), (byte)((x >> 8) & 0xFF), (byte)(x & 0xFF)]);

    [Fact]
    public void ReadIndexData0Test()
    {
        var stream = new MemoryStream();
        stream.WriteUShortByBigEndian(0);
        stream.Position = 0;

        var result = CompactFontFormat.ReadIndexData(stream);
        Assert.Equal(result.Length, 0);
    }

    [Fact]
    public void ReadIndexData1_0Test()
    {
        var stream = new MemoryStream();
        stream.WriteUShortByBigEndian(1); // count
        stream.WriteByte(1); // offset size
        stream.WriteByte(1); // offset[0]
        stream.WriteByte(1); // offset[1]
        stream.Position = 0;

        var result = CompactFontFormat.ReadIndexData(stream);
        Assert.Equal(result.Length, 1);
        Assert.Equal(result[0].Length, 0);
    }

    [Fact]
    public void ReadIndexData2_0_254Test()
    {
        var stream = new MemoryStream();
        stream.WriteUShortByBigEndian(2); // count
        stream.WriteByte(1); // offset size
        stream.WriteByte(1); // offset[0]
        stream.WriteByte(1); // offset[1]
        stream.WriteByte(255); // offset[2]
        stream.Write([.. MakeBytes(254)]);
        stream.Position = 0;

        var result = CompactFontFormat.ReadIndexData(stream);
        Assert.Equal(result.Length, 2);
        Assert.Equal(result[0].Length, 0);
        Assert.Equal(result[1].Length, 254);
        Assert.Equal(result[1], MakeBytes(254));
    }

    [Fact]
    public void ReadIndexData2_0_255Test()
    {
        var stream = new MemoryStream();
        stream.WriteUShortByBigEndian(2); // count
        stream.WriteByte(2); // offset size
        stream.WriteUShortByBigEndian(1); // offset[0]
        stream.WriteUShortByBigEndian(1); // offset[1]
        stream.WriteUShortByBigEndian(256); // offset[2]
        stream.Write([.. MakeBytes(255)]);
        stream.Position = 0;

        var result = CompactFontFormat.ReadIndexData(stream);
        Assert.Equal(result.Length, 2);
        Assert.Equal(result[0].Length, 0);
        Assert.Equal(result[1].Length, 255);
        Assert.Equal(result[1], MakeBytes(255));
    }

    [Fact]
    public void ReadIndexData3_0_255_65279Test()
    {
        var stream = new MemoryStream();
        stream.WriteUShortByBigEndian(3); // count
        stream.WriteByte(2); // offset size
        stream.WriteUShortByBigEndian(1); // offset[0]
        stream.WriteUShortByBigEndian(1); // offset[1]
        stream.WriteUShortByBigEndian(256); // offset[2]
        stream.WriteUShortByBigEndian(65535); // offset[3]
        stream.Write([.. MakeBytes(255)]);
        stream.Write([.. MakeBytes(65279)]);
        stream.Position = 0;

        var result = CompactFontFormat.ReadIndexData(stream);
        Assert.Equal(result.Length, 3);
        Assert.Equal(result[0].Length, 0);
        Assert.Equal(result[1].Length, 255);
        Assert.Equal(result[1], MakeBytes(255));
        Assert.Equal(result[2].Length, 65279);
        Assert.Equal(result[2], MakeBytes(65279));
    }

    [Fact]
    public void ReadIndexData3_0_255_65280Test()
    {
        var stream = new MemoryStream();
        stream.WriteUShortByBigEndian(3); // count
        stream.WriteByte(3); // offset size
        Write3Bytes(stream, 1); // offset[0]
        Write3Bytes(stream, 1); // offset[1]
        Write3Bytes(stream, 256); // offset[2]
        Write3Bytes(stream, 65536); // offset[3]
        stream.Write([.. MakeBytes(255)]);
        stream.Write([.. MakeBytes(65280)]);
        stream.Position = 0;

        var result = CompactFontFormat.ReadIndexData(stream);
        Assert.Equal(result.Length, 3);
        Assert.Equal(result[0].Length, 0);
        Assert.Equal(result[1].Length, 255);
        Assert.Equal(result[1], MakeBytes(255));
        Assert.Equal(result[2].Length, 65280);
        Assert.Equal(result[2], MakeBytes(65280));
    }

    [Fact]
    public void ReadIndexData4_0_255_65280_16711679Test()
    {
        var stream = new MemoryStream();
        stream.WriteUShortByBigEndian(4); // count
        stream.WriteByte(3); // offset size
        Write3Bytes(stream, 1); // offset[0]
        Write3Bytes(stream, 1); // offset[1]
        Write3Bytes(stream, 256); // offset[2]
        Write3Bytes(stream, 65536); // offset[3]
        Write3Bytes(stream, 16777215); // offset[4]
        stream.Write([.. MakeBytes(255)]);
        stream.Write([.. MakeBytes(65280)]);
        stream.Write([.. MakeBytes(16711679)]);
        stream.Position = 0;

        var result = CompactFontFormat.ReadIndexData(stream);
        Assert.Equal(result.Length, 4);
        Assert.Equal(result[0].Length, 0);
        Assert.Equal(result[1].Length, 255);
        Assert.Equal(result[1], MakeBytes(255));
        Assert.Equal(result[2].Length, 65280);
        Assert.Equal(result[2], MakeBytes(65280));
        Assert.Equal(result[3].Length, 16711679);
        Assert.Equal(result[3], MakeBytes(16711679));
    }

    [Fact]
    public void ReadIndexData4_0_255_65280_16711680Test()
    {
        var stream = new MemoryStream();
        stream.WriteUShortByBigEndian(4); // count
        stream.WriteByte(4); // offset size
        stream.WriteUIntByBigEndian(1); // offset[0]
        stream.WriteUIntByBigEndian(1); // offset[1]
        stream.WriteUIntByBigEndian(256); // offset[2]
        stream.WriteUIntByBigEndian(65536); // offset[3]
        stream.WriteUIntByBigEndian(16777216); // offset[4]
        stream.Write([.. MakeBytes(255)]);
        stream.Write([.. MakeBytes(65280)]);
        stream.Write([.. MakeBytes(16711680)]);
        stream.Position = 0;

        var result = CompactFontFormat.ReadIndexData(stream);
        Assert.Equal(result.Length, 4);
        Assert.Equal(result[0].Length, 0);
        Assert.Equal(result[1].Length, 255);
        Assert.Equal(result[1], MakeBytes(255));
        Assert.Equal(result[2].Length, 65280);
        Assert.Equal(result[2], MakeBytes(65280));
        Assert.Equal(result[3].Length, 16711680);
        Assert.Equal(result[3], MakeBytes(16711680));
    }
}
