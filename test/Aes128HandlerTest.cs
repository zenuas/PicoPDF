using Pdf.Documents.Security;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Xunit;

namespace PicoPDF.Test;

public class Aes128HandlerTest
{
    [Fact]
    public void InitializeWithoutGenerateIV()
    {
        using var aes = Aes128Handler.InitializeWithoutGenerateIV(
            [0x08, 0xe2, 0x08, 0x4d, 0xb2, 0x49, 0xdc, 0x76, 0x13, 0x0f, 0xad, 0x44, 0x13, 0x08, 0xc8, 0xd9], 5, 0);
        Assert.Equal(aes.Key, [0x3f, 0x52, 0x67, 0x3b, 0x7b, 0xe3, 0xbb, 0x49, 0x54, 0xb1, 0x7f, 0x38, 0x1a, 0xfa, 0x86, 0x99]);
    }

    public static async Task<byte[]> PipeReadAsync(PipeReader reader)
    {
        var buffer = new List<byte>();
        while (true)
        {
            var result = await reader.ReadAsync();
            if (result.IsCanceled) throw new OperationCanceledException();
            if (result.Buffer.IsEmpty) break;

            buffer.AddRange(result.Buffer.ToArray());
            reader.AdvanceTo(result.Buffer.End);
        }
        return [.. buffer];
    }

    [Fact]
    public async Task DecryptPipeTest()
    {
        var handler = new Aes128Handler { Key = [0x08, 0xe2, 0x08, 0x4d, 0xb2, 0x49, 0xdc, 0x76, 0x13, 0x0f, 0xad, 0x44, 0x13, 0x08, 0xc8, 0xd9] };
        var encrypter1 = handler.CreateEncrypterPipe(5, 0);
        var decrypter1 = handler.CreateDecrypterPipe(5, 0);

        var null_text = new byte[] { };
        encrypter1.Input.Write(null_text);
        encrypter1.Input.Complete();
        var cipher_text1 = await PipeReadAsync(encrypter1.Output);
        Assert.Equal(cipher_text1.Length, 32);
        decrypter1.Input.Write(cipher_text1);
        decrypter1.Input.Complete();
        var plain_text1 = await PipeReadAsync(decrypter1.Output);
        Assert.Equal(plain_text1, null_text);

        var encrypter2 = handler.CreateEncrypterPipe(5, 0);
        var decrypter2 = handler.CreateDecrypterPipe(5, 0);

        var short_text = new byte[] { 0x48, 0x65, 0x6c, 0x24, 0x97 };
        encrypter2.Input.Write(short_text);
        encrypter2.Input.Complete();
        var cipher_text2 = await PipeReadAsync(encrypter2.Output);
        decrypter2.Input.Write(cipher_text2);
        decrypter2.Input.Complete();
        var plain_text2 = await PipeReadAsync(decrypter2.Output);
        Assert.Equal(plain_text2, short_text);

        var encrypter3 = handler.CreateEncrypterPipe(5, 0);
        var decrypter3 = handler.CreateDecrypterPipe(5, 0);

        var long_text = new byte[] { 0x48, 0x65, 0x6c, 0x24, 0x97, 0x04, 0xf1, 0xeb, 0x17, 0x22, 0x88, 0xc9, 0x9d, 0x90, 0x45, 0x39, 0x4a, 0x07, 0x4c };
        encrypter3.Input.Write(long_text);
        encrypter3.Input.Complete();
        var cipher_text3 = await PipeReadAsync(encrypter3.Output);
        decrypter3.Input.Write(cipher_text3);
        decrypter3.Input.Complete();
        var plain_text3 = await PipeReadAsync(decrypter3.Output);
        Assert.Equal(plain_text3, long_text);
    }
    [Fact]
    public void DecryptFunctionTest()
    {
        var handler = new Aes128Handler { Key = [0x08, 0xe2, 0x08, 0x4d, 0xb2, 0x49, 0xdc, 0x76, 0x13, 0x0f, 0xad, 0x44, 0x13, 0x08, 0xc8, 0xd9] };
        using var encrypter = handler.CreateEncrypterConverter(5, 0);
        using var decrypter = handler.CreateDecrypterConverter(5, 0);

        var null_text = new byte[] { };
        var cipher_text1 = encrypter.Convert(null_text);
        Assert.Equal(cipher_text1.Length, 32);
        var plain_text1 = decrypter.Convert(cipher_text1);
        Assert.Equal(plain_text1, null_text);

        var short_text = new byte[] { 0x48, 0x65, 0x6c, 0x24, 0x97 };
        var cipher_text2 = encrypter.Convert(short_text);
        var plain_text2 = decrypter.Convert(cipher_text2);
        Assert.Equal(plain_text2, short_text);

        var long_text = new byte[] { 0x48, 0x65, 0x6c, 0x24, 0x97, 0x04, 0xf1, 0xeb, 0x17, 0x22, 0x88, 0xc9, 0x9d, 0x90, 0x45, 0x39, 0x4a, 0x07, 0x4c };
        var cipher_text3 = encrypter.Convert(long_text);
        var plain_text3 = decrypter.Convert(cipher_text3);
        Assert.Equal(plain_text3, long_text);
    }
}
