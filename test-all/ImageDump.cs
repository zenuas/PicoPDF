using Image;
using Mina.Extension;
using System;

namespace PicoPDF.TestAll;

public class ImageDump : ICommand
{
    public void Run(string[] args)
    {
        var image = ImageLoader.FromFile(args[0])!.Cast<IImageCanvas>();
        for (int y = 0; y < image.Height; y++)
        {
            var row = image.Canvas[y];
            Console.Write($"[");
            for (int x = 0; x < image.Width; x++)
            {
                if (x > 0) Console.Write(", ");
                var color = row[x];
                Console.Write($"0x{color.R:X2}{color.G:X2}{color.B:X2}");
            }
            Console.WriteLine("],");
        }
    }
}
