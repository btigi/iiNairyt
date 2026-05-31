using ii.Nairyt.Decoding;
using SixLabors.ImageSharp.PixelFormats;

namespace ii.Nairyt;

public static class TyrianPalette
{
    public static Rgba32[] Load(string palettePath, int paletteIndex = 0)
    {
        ArgumentException.ThrowIfNullOrEmpty(palettePath);

        var data = File.ReadAllBytes(palettePath);
        var requiredLength = (paletteIndex + 1) * TyrianConstants.PaletteSize;

        if (data.Length < requiredLength)
        {
            throw new InvalidDataException($"Palette file too small for index {paletteIndex}: {palettePath}");
        }

        var colors = new Rgba32[TyrianConstants.PaletteColorCount];
        var offset = paletteIndex * TyrianConstants.PaletteSize;

        for (var i = 0; i < TyrianConstants.PaletteColorCount; i++)
        {
            var red = data[offset++] * 255 / TyrianConstants.PaletteMaxComponent;
            var green = data[offset++] * 255 / TyrianConstants.PaletteMaxComponent;
            var blue = data[offset++] * 255 / TyrianConstants.PaletteMaxComponent;

            colors[i] = i == 0 ? new Rgba32(0, 0, 0, 0) : new Rgba32((byte)red, (byte)green, (byte)blue, 255);
        }

        return colors;
    }
}