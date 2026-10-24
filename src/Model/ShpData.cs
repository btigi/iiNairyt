using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace ii.Nairyt.Model;

public enum ShpBankFormat
{
    SpriteSheet,
    Estsc,
}

public sealed record ShpSprite(int Width, int Height, byte[] Pixels)
{
    public static ShpSprite FromRgba(Image<Rgba32> image, Rgba32[] palette)
    {
        ArgumentNullException.ThrowIfNull(image);
        ArgumentNullException.ThrowIfNull(palette);

        if (palette.Length is < 2 or > byte.MaxValue + 1)
        {
            throw new ArgumentException("Palette must contain between 2 and 256 colors.", nameof(palette));
        }

        var pixels = new byte[image.Width * image.Height];

        for (var y = 0; y < image.Height; y++)
        {
            var row = y * image.Width;

            for (var x = 0; x < image.Width; x++)
            {
                var pixel = image[x, y];
                pixels[row + x] = pixel.A == 0 ? (byte)0 : NearestPaletteIndex(pixel, palette);
            }
        }

        return new ShpSprite(image.Width, image.Height, pixels);
    }

    private static byte NearestPaletteIndex(Rgba32 pixel, Rgba32[] palette)
    {
        var bestIndex = 1;
        var bestDistance = int.MaxValue;

        for (var index = 1; index < palette.Length; index++)
        {
            var candidate = palette[index];
            var red = pixel.R - candidate.R;
            var green = pixel.G - candidate.G;
            var blue = pixel.B - candidate.B;
            var distance = (red * red) + (green * green) + (blue * blue);

            if (distance >= bestDistance)
            {
                continue;
            }

            bestIndex = index;
            bestDistance = distance;

            if (distance == 0)
            {
                break;
            }
        }

        return (byte)bestIndex;
    }
}

public sealed record ShpBank(ShpBankFormat Format, IReadOnlyList<ShpSprite?> Sprites);

public sealed record ShpData(ShpFormat Format, IReadOnlyList<ShpBank> Banks);
