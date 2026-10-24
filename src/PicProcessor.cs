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

    public static void Write(IReadOnlyList<ExportedImage> images, string picPath, string palettePath)
    {
        ArgumentNullException.ThrowIfNull(images);
        ArgumentException.ThrowIfNullOrEmpty(picPath);
        ArgumentException.ThrowIfNullOrEmpty(palettePath);

        if (!File.Exists(palettePath))
        {
            throw new FileNotFoundException($"Palette file not found: {palettePath}", palettePath);
        }

        if (images.Count != TyrianConstants.PicImageCount)
        {
            throw new InvalidDataException($"PIC files contain {TyrianConstants.PicImageCount} images.");
        }

        var paletteCache = new Dictionary<int, Rgba32[]>();
        var indexed = new byte[images.Count][];

        for (var imageIndex = 0; imageIndex < images.Count; imageIndex++)
        {
            var exported = images[imageIndex] ?? throw new InvalidDataException($"PIC image {imageIndex} is missing.");

            if (exported.Image.Width != TyrianConstants.PicWidth || exported.Image.Height != TyrianConstants.PicHeight)
            {
                throw new InvalidDataException(
                    $"PIC image {imageIndex} must be {TyrianConstants.PicWidth}x{TyrianConstants.PicHeight}.");
            }

            var paletteIndex = TyrianConstants.PicPaletteIndices[imageIndex];

            if (!paletteCache.TryGetValue(paletteIndex, out var palette))
            {
                palette = TyrianPalette.Load(palettePath, paletteIndex);
                paletteCache[paletteIndex] = palette;
            }

            indexed[imageIndex] = ShpSprite.FromRgba(exported.Image, palette).Pixels;
        }

        File.WriteAllBytes(picPath, TyrianPicEncoder.Encode(indexed));
    }
}