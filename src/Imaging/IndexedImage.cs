using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace ii.Nairyt.Imaging;

internal class IndexedImage(int width, int height)
{
    public int Width { get; } = width;

    public int Height { get; } = height;

    public byte[] Pixels { get; } = new byte[width * height];

    public void SetPixel(int x, int y, byte paletteIndex)
    {
        if ((uint)x >= (uint)Width || (uint)y >= (uint)Height)
        {
            return;
        }

        Pixels[(y * Width) + x] = paletteIndex;
    }

    public Image<Rgba32> ToRgba32(Rgba32[] palette)
    {
        var image = new Image<Rgba32>(Width, Height);

        for (var y = 0; y < Height; y++)
        {
            var rowOffset = y * Width;

            for (var x = 0; x < Width; x++)
            {
                var paletteIndex = Pixels[rowOffset + x];
                image[x, y] = palette[paletteIndex];
            }
        }

        return image;
    }
}
