using Avalonia.Media;
using SkiaSharp;

namespace PicNetStudio.Avalonia.Utils;

public static class SKUtils {
    public static SKColor AvToSkia(Color c) => new(c.R, c.G, c.B, c.A);

    public static Color SkiaToAv(SKColor c) => new(c.Alpha, c.Red, c.Green, c.Blue);
}