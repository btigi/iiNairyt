using ii.Nairyt.Decoding;
using ii.Nairyt.Imaging;
using ii.Nairyt.Model;
using SixLabors.ImageSharp.PixelFormats;

namespace ii.Nairyt;

public static class ShpProcessor
{
    public static ShpFormat DetectFormat(string filePath, ReadOnlySpan<byte> data)
    {
        ArgumentException.ThrowIfNullOrEmpty(filePath);

        return ShpFormatDetector.Detect(filePath, data);
    }

    public static IReadOnlyList<ExportedImage> Read(string shpPath, string palettePath)
    {
        ArgumentException.ThrowIfNullOrEmpty(shpPath);
        ArgumentException.ThrowIfNullOrEmpty(palettePath);

        var data = File.ReadAllBytes(shpPath);
        var format = DetectFormat(shpPath, data);
        var baseName = Path.GetFileNameWithoutExtension(shpPath);
        var defaultPalette = TyrianPalette.Load(palettePath);
        var exports = new List<ExportedImage>();

        switch (format)
        {
            case ShpFormat.SpriteSheet:
                AppendIndexedSprites(exports, baseName, SpriteSheetDecoder.DecodeAll(data), defaultPalette);
                break;
            case ShpFormat.Estsc:
                var decoder = new EstscDecoder(data);
                AppendIndexedSprites(exports, baseName, decoder.LoadImages(), defaultPalette);
                break;
            case ShpFormat.TyrianComposite:
                foreach (var decodedImage in TyrianCompositeDecoder.Decode(data, palettePath))
                {
                    exports.Add(new ExportedImage($"{baseName}-{decodedImage.Name}", decodedImage.Image.ToRgba32(decodedImage.Palette)));
                }
                break;
            default:
                throw new InvalidOperationException($"Unsupported SHP format: {format}");
        }

        return exports;
    }

    private static void AppendIndexedSprites(List<ExportedImage> exports, string baseName, IReadOnlyList<IndexedImage?> sprites, Rgba32[] palette)
    {
        for (var index = 0; index < sprites.Count; index++)
        {
            var sprite = sprites[index];

            if (sprite is null)
            {
                continue;
            }

            exports.Add(new ExportedImage($"{baseName}-{index}", sprite.ToRgba32(palette)));
        }
    }
}