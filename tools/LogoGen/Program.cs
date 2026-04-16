using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

var sourcePath = @"C:\Users\ibrah\Desktop\AI Projects\AcademicAI\src\AcademicAI.App\assets\logo_temp.png";
var outputPath = @"C:\Users\ibrah\Desktop\AI Projects\AcademicAI\src\AcademicAI.App\assets\app.ico";

using var sourceImage = new Bitmap(sourcePath);
var sizes = new[] { 16, 24, 32, 48, 64, 128, 256 };
var bitmaps = sizes.Select(size => ResizeImage(sourceImage, size, size)).ToList();

using var ms = new MemoryStream();
using var writer = new BinaryWriter(ms);

writer.Write((short)0);
writer.Write((short)1);
writer.Write((short)sizes.Length);

var offset = 6 + 16 * sizes.Length;

foreach (var bmp in bitmaps)
{
    writer.Write((byte)(bmp.Width >= 256 ? 0 : bmp.Width));
    writer.Write((byte)(bmp.Height >= 256 ? 0 : bmp.Height));
    writer.Write((byte)0);
    writer.Write((byte)0);
    writer.Write((short)1);
    writer.Write((short)32);
    using var pngStream = new MemoryStream();
    bmp.Save(pngStream, ImageFormat.Png);
    var data = pngStream.ToArray();
    writer.Write(data.Length);
    writer.Write(offset);
    offset += data.Length;
}

foreach (var bmp in bitmaps)
{
    using var pngStream = new MemoryStream();
    bmp.Save(pngStream, ImageFormat.Png);
    writer.Write(pngStream.ToArray());
}

File.WriteAllBytes(outputPath, ms.ToArray());
Console.WriteLine("Icon generated from downloaded PNG successfully");

static Bitmap ResizeImage(Image source, int width, int height)
{
    var bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
    using var g = Graphics.FromImage(bmp);
    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
    g.DrawImage(source, 0, 0, width, height);
    return bmp;
}
