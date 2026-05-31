using System.Buffers.Binary;
using ii.Nairyt.Imaging;
using SixLabors.ImageSharp.PixelFormats;

namespace ii.Nairyt.Decoding;

internal static class TyrianCompositeDecoder
{
    private static readonly int[] PortraitPaletteIndices =
    [
        1, 2, 3, 4, 6, 9, 11, 12, 10, 13, 14, 15,
    ];

    public static IEnumerable<DecodedImage> Decode(byte[] data, string palettePath)
    {
        var sectionCount = BinaryPrimitives.ReadUInt16LittleEndian(data);
        var sectionPositions = ReadSectionPositions(data, sectionCount);
        var decoder = new EstscDecoder(data);
        var defaultPalette = TyrianPalette.Load(palettePath);

        var estscSectionCount = Math.Min(TyrianConstants.EstscSectionCount, (int)sectionCount);

        for (var section = 0; section < estscSectionCount; section++)
        {
            decoder.Seek(sectionPositions[section]);

            foreach (var decodedImage in ExportEstscSection(decoder.LoadImages(), section, palettePath, defaultPalette))
            {
                yield return decodedImage;
            }
        }

        var spriteSheetSectionEnd = Math.Min(TyrianConstants.SpriteSheetSectionEnd, (int)sectionCount);

        for (var section = TyrianConstants.SpriteSheetSectionStart; section < spriteSheetSectionEnd; section++)
        {
            var sectionLength = sectionPositions[section + 1] - sectionPositions[section];
            var sectionData = data.AsSpan(sectionPositions[section], sectionLength);
            var sprites = SpriteSheetDecoder.DecodeAll(sectionData);

            for (var index = 0; index < sprites.Count; index++)
            {
                var sprite = sprites[index];

                if (sprite is null)
                {
                    continue;
                }

                yield return new DecodedImage($"{section}-{index}", sprite, defaultPalette);
            }
        }
    }

    private static int[] ReadSectionPositions(byte[] data, ushort sectionCount)
    {
        var positions = new int[sectionCount + 1];

        for (var i = 0; i < sectionCount; i++)
        {
            positions[i] = (int)BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(sizeof(ushort) + (i * sizeof(uint))));
        }

        positions[sectionCount] = data.Length;
        return positions;
    }

    private static IEnumerable<DecodedImage> ExportEstscSection(IReadOnlyList<IndexedImage?> images, int section, string palettePath, Rgba32[] defaultPalette)
    {
        for (var index = 0; index < images.Count; index++)
        {
            var image = images[index];

            if (image is null)
            {
                continue;
            }

            var palette = section == TyrianConstants.PortraitSectionIndex && index < PortraitPaletteIndices.Length ? TyrianPalette.Load(palettePath, PortraitPaletteIndices[index]) : defaultPalette;

            yield return new DecodedImage($"{section}-{index}", image, palette);
        }
    }
}

internal readonly record struct DecodedImage(string Name, IndexedImage Image, Rgba32[] Palette);