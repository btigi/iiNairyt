using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace ii.Nairyt.Model;

public sealed record ShapeDatData(int Width, int Height, byte[] Pixels, byte[] Trailer)
{
    public static ShapeDatData FromRgba(Image<Rgba32> image, Rgba32[] palette, byte[]? trailer = null)
    {
        var sprite = ShpSprite.FromRgba(image, palette);
        return new ShapeDatData(sprite.Width, sprite.Height, sprite.Pixels, trailer ?? []);
    }
}