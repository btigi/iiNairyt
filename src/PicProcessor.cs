using ii.Nairyt.Decoding;
using ii.Nairyt.Model;
using SixLabors.ImageSharp.PixelFormats;

namespace ii.Nairyt;

public static class PicProcessor
{
    public static IReadOnlyList<ExportedImage> Read(string picPath, string palettePath)
    {
        ArgumentException.ThrowIfNullOrEmpty(picPath);
        ArgumentException.ThrowIfNullOrEmpty(palettePath);

        if (!File.Exists(picPath))
        {
            throw new FileNotFoundException($"PIC file not found: {picPath}", picPath);
        }

        if (!File.Exists(palettePath))
        {
            throw new FileNotFoundException($"Palette file not found: {palettePath}", palettePath);
        }

        var data = File.ReadAllBytes(picPath);
        var baseName = Path.GetFileNameWithoutExtension(picPath);
        var images = TyrianPicDecoder.DecodeAll(data);
        var paletteCache = new Dictionary<int, Rgba32[]>();
        var exports = new List<ExportedImage>(images.Count);

        for (var imageIndex = 0; imageIndex < images.Count; imageIndex++)
        {
            var paletteIndex = TyrianConstants.PicPaletteIndices[imageIndex];

            if (!paletteCache.TryGetValue(paletteIndex, out var palette))
            {
                palette = TyrianPalette.Load(palettePath, paletteIndex);
                paletteCache[paletteIndex] = palette;
            }

            exports.Add(new ExportedImage($"{baseName}-{imageIndex}", images[imageIndex].ToRgba32(palette)));
        }

        return exports;
    }
}