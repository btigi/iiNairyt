using System.Buffers.Binary;
using ii.Nairyt.Imaging;
using ii.Nairyt.Model;
using SixLabors.ImageSharp.PixelFormats;

namespace ii.Nairyt.Decoding;

internal static class TyrianCompositeDecoder
{
    public static IReadOnlyList<DecodedShpBank> DecodeBanks(byte[] data)
    {
        var sectionCount = BinaryPrimitives.ReadUInt16LittleEndian(data);
        var sectionPositions = ReadSectionPositions(data, sectionCount);
        var decoder = new EstscDecoder(data);
        var banks = new List<DecodedShpBank>(sectionCount);

        for (var section = 0; section < sectionCount; section++)
        {
            if (section < TyrianConstants.EstscSectionCount)
            {
                decoder.Seek(sectionPositions[section]);
                banks.Add(new DecodedShpBank(ShpBankFormat.Estsc, decoder.LoadImages()));
                continue;
            }

            var sectionLength = sectionPositions[section + 1] - sectionPositions[section];
            var sectionData = data.AsSpan(sectionPositions[section], sectionLength);
            banks.Add(new DecodedShpBank(ShpBankFormat.SpriteSheet, SpriteSheetDecoder.DecodeAll(sectionData)));
        }

        return banks;
    }

    public static IEnumerable<DecodedImage> Decode(byte[] data, string palettePath)
    {
        var defaultPalette = TyrianPalette.Load(palettePath);
        var banks = DecodeBanks(data);

        for (var section = 0; section < banks.Count; section++)
        {
            var sprites = banks[section].Sprites;

            for (var index = 0; index < sprites.Count; index++)
            {
                var sprite = sprites[index];

                if (sprite is null)
                {
                    continue;
                }

                yield return new DecodedImage($"{section}-{index}", sprite, PaletteFor(section, index, palettePath, defaultPalette));
            }
        }
    }

    public static Rgba32[] PaletteFor(int section, int spriteIndex, string palettePath, Rgba32[] defaultPalette)
    {
        if (section == TyrianConstants.PortraitSectionIndex && spriteIndex < TyrianConstants.PortraitPaletteIndices.Length)
        {
            return TyrianPalette.Load(palettePath, TyrianConstants.PortraitPaletteIndices[spriteIndex]);
        }

        return defaultPalette;
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
}

internal readonly record struct DecodedShpBank(ShpBankFormat Format, IReadOnlyList<IndexedImage?> Sprites);

internal readonly record struct DecodedImage(string Name, IndexedImage Image, Rgba32[] Palette);