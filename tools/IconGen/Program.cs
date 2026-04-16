using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

var sizes = new[] { 16, 32, 48, 256 };
var bitmaps = sizes.Select(s =>
{
    var bmp = new Bitmap(s, s);
    using var g = Graphics.FromImage(bmp);
    g.Clear(Color.FromArgb(107, 86, 217));
    var fontSize = Math.Max(s / 4, 6);
    using var font = new Font("Segoe UI", fontSize, FontStyle.Bold);
    var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
    g.DrawString("A", font, Brushes.White, new RectangleF(0, 0, s, s), sf);
    return bmp;
}).ToList();

using var ms = new MemoryStream();
using var writer = new BinaryWriter(ms);
writer.Write((short)0);
writer.Write((short)1);
writer.Write((short)4);

var offset = 6 + 16 * 4;

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

File.WriteAllBytes(@"C:\Users\ibrah\Desktop\AI Projects\AcademicAI\src\AcademicAI.App\assets\app.ico", ms.ToArray());
Console.WriteLine("Icon created successfully");
