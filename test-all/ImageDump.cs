using Image;
using Mina.Extension;
using System;
using System.IO;

namespace PicoPDF.TestAll;

public class ImageDump : ICommand
{
    public void Run(string[] args)
    {
        foreach (string arg in args)
        {
            var image = ImageLoader.FromFile(arg)!.Cast<IImageCanvas>();
            var filename = Path.GetFileNameWithoutExtension(arg);
            Console.WriteLine($"    [Fact]");
            Console.WriteLine($"    public void Load_{filename}()");
            Console.WriteLine($"    {{");
            Console.WriteLine($"        var image = ImageLoader.FromFile($\"{{TestDirectory}}/{filename}.png\");");
            Console.WriteLine($"        var png = Assert.IsType<PngFile>(image);");
            Console.WriteLine($"        Assert.Equal(png.Width, {image.Width});");
            Console.WriteLine($"        Assert.Equal(png.Height, {image.Height});");
            Console.WriteLine($"        Assert.Equal(png.Canvas.Length, {image.Canvas.Length});");
            Console.WriteLine($"        ");
            Console.Write($"        //                                          pos  |");
            for (int x = 0; x < image.Width; x++) Console.Write($" {x,-7} |");
            Console.WriteLine($"");
            for (int y = 0; y < image.Height; y++)
            {
                var row = image.Canvas[y];
                Console.Write($"        Assert.Equal<int[]>(ColorsToInts(png.Canvas[{y}]), [");
                for (int x = 0; x < image.Width; x++)
                {
                    if (x > 0) Console.Write(", ");
                    var color = row[x].ToColor();
                    Console.Write($"0x{color.R:X2}{color.G:X2}{color.B:X2}");
                }
                Console.WriteLine("]);");
            }
            Console.WriteLine($"    }}");
            Console.WriteLine($"    ");
        }
    }
}
