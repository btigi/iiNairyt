using ii.Nairyt.Decoding;
using ii.Nairyt.Imaging;
using ii.Nairyt.Model;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace ii.Nairyt;

public static class ShpProcessor
{
    public static ShpFormat DetectFormat(string filePath, ReadOnlySpan<byte> data)
    {
        ArgumentException.ThrowIfNullOrEmpty(filePath);

        return ShpFormatDetector.Detect(filePath, data);
    }

    public static ShpData Read(string shpPath)
    {
        ArgumentException.ThrowIfNullOrEmpty(shpPath);

        if (!File.Exists(shpPath))
        {
            throw new FileNotFoundException($"SHP file not found: {shpPath}", shpPath);
        }

        var data = File.ReadAllBytes(shpPath);
        var format = DetectFormat(shpPath, data);

        return format switch
        {
            ShpFormat.SpriteSheet => new ShpData(format, [ToBank(ShpBankFormat.SpriteSheet, SpriteSheetDecoder.DecodeAll(data))]),
            ShpFormat.Estsc => new ShpData(format, [ToBank(ShpBankFormat.Estsc, new EstscDecoder(data).LoadImages())]),
            ShpFormat.TyrianComposite => new ShpData(format, ToBanks(TyrianCompositeDecoder.DecodeBanks(data))),
            _ => throw new InvalidOperationException($"Unsupported SHP format: {format}"),
        };
    }

    public static void Write(ShpData shp, string shpPath)
    {
        ArgumentNullException.ThrowIfNull(shp);
        ArgumentException.ThrowIfNullOrEmpty(shpPath);

        File.WriteAllBytes(shpPath, Encode(shp));
    }

    public static IReadOnlyList<ExportedImage> Export(ShpData shp, string palettePath, string name)
    {
        ArgumentNullException.ThrowIfNull(shp);
        ArgumentException.ThrowIfNullOrEmpty(palettePath);
        ArgumentException.ThrowIfNullOrEmpty(name);

        var defaultPalette = TyrianPalette.Load(palettePath);
        var includeBankIndex = shp.Format == ShpFormat.TyrianComposite;
        var exports = new List<ExportedImage>();

        for (var bankIndex = 0; bankIndex < shp.Banks.Count; bankIndex++)
        {
            var sprites = shp.Banks[bankIndex].Sprites;

            for (var spriteIndex = 0; spriteIndex < sprites.Count; spriteIndex++)
            {
                var sprite = sprites[spriteIndex];

                if (sprite is null)
                {
                    continue;
                }

                var palette = includeBankIndex
                    ? TyrianCompositeDecoder.PaletteFor(bankIndex, spriteIndex, palettePath, defaultPalette)
                    : defaultPalette;
                var spriteName = includeBankIndex
                    ? $"{name}-{bankIndex}-{spriteIndex}"
                    : $"{name}-{spriteIndex}";

                exports.Add(new ExportedImage(spriteName, ToRgba(sprite, palette)));
            }
        }

        return exports;
    }

    private static byte[] Encode(ShpData shp)
    {
        ArgumentNullException.ThrowIfNull(shp.Banks);

        return shp.Format switch
        {
            ShpFormat.SpriteSheet => SpriteSheetEncoder.Encode(SingleBank(shp, ShpBankFormat.SpriteSheet).Sprites),
            ShpFormat.Estsc => EstscEncoder.Encode(SingleBank(shp, ShpBankFormat.Estsc).Sprites),
            ShpFormat.TyrianComposite => EncodeComposite(shp.Banks),
            _ => throw new InvalidOperationException($"Unsupported SHP format: {shp.Format}"),
        };
    }

    private static byte[] EncodeComposite(IReadOnlyList<ShpBank> banks)
    {
        if (banks.Count == TyrianConstants.SpriteSheetSectionEnd)
        {
            for (var index = 0; index < TyrianConstants.EstscSectionCount; index++)
            {
                var bank = banks[index] ?? throw new InvalidDataException($"Composite SHP bank {index} is missing.");

                if (bank.Format != ShpBankFormat.Estsc)
                {
                    throw new InvalidDataException($"Composite SHP bank {index} must be an ESTSC bank.");
                }
            }

            for (var index = TyrianConstants.SpriteSheetSectionStart; index < banks.Count; index++)
            {
                var bank = banks[index] ?? throw new InvalidDataException($"Composite SHP bank {index} is missing.");

                if (bank.Format != ShpBankFormat.SpriteSheet)
                {
                    throw new InvalidDataException($"Composite SHP bank {index} must be a sprite sheet bank.");
                }
            }
        }

        return TyrianCompositeEncoder.Encode(banks);
    }

    private static ShpBank SingleBank(ShpData shp, ShpBankFormat format)
    {
        if (shp.Banks.Count != 1 || shp.Banks[0].Format != format)
        {
            throw new InvalidDataException($"{shp.Format} SHP data must contain one {format} bank.");
        }

        return shp.Banks[0];
    }

    private static IReadOnlyList<ShpBank> ToBanks(IReadOnlyList<DecodedShpBank> banks)
    {
        var converted = new ShpBank[banks.Count];

        for (var index = 0; index < banks.Count; index++)
        {
            converted[index] = ToBank(banks[index].Format, banks[index].Sprites);
        }

        return converted;
    }

    private static ShpBank ToBank(ShpBankFormat format, IReadOnlyList<IndexedImage?> sprites)
    {
        var converted = new ShpSprite?[sprites.Count];

        for (var index = 0; index < sprites.Count; index++)
        {
            var sprite = sprites[index];

            if (sprite is null)
            {
                continue;
            }

            converted[index] = new ShpSprite(sprite.Width, sprite.Height, sprite.Pixels.ToArray());
        }

        return new ShpBank(format, converted);
    }

    private static Image<Rgba32> ToRgba(ShpSprite sprite, Rgba32[] palette)
    {
        var image = new IndexedImage(sprite.Width, sprite.Height);
        sprite.Pixels.CopyTo(image.Pixels.AsSpan());
        return image.ToRgba32(palette);
    }
}
