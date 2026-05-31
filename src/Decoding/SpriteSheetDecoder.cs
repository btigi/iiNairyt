using System.Buffers.Binary;
using ii.Nairyt.Imaging;

namespace ii.Nairyt.Decoding;

internal static class SpriteSheetDecoder
{
    public static IReadOnlyList<IndexedImage?> DecodeAll(ReadOnlySpan<byte> data)
    {
        var lastOffset = BinaryPrimitives.ReadUInt16LittleEndian(data);
        var spriteCount = lastOffset / sizeof(ushort);
        var sprites = new IndexedImage?[spriteCount];

        for (var index = 0; index < spriteCount; index++)
        {
            var spriteOffset = BinaryPrimitives.ReadUInt16LittleEndian(data[(index * sizeof(ushort))..]);
            sprites[index] = DecodeSprite(data, spriteOffset);
        }

        return sprites;
    }

    private static IndexedImage? DecodeSprite(ReadOnlySpan<byte> data, int offset)
    {
        var rows = new List<byte[]>();
        var currentRow = new List<byte>();
        var maxWidth = 0;

        while (offset < data.Length && data[offset] != TyrianConstants.SpriteRowTerminator)
        {
            var control = data[offset++];
            var transparentPixels = control & 0x0F;
            var coloredPixels = control >> 4;

            for (var i = 0; i < transparentPixels; i++)
            {
                currentRow.Add(0);
            }

            if (coloredPixels == 0)
            {
                maxWidth = Math.Max(maxWidth, currentRow.Count);
                rows.Add(currentRow.ToArray());
                currentRow = [];
                continue;
            }

            for (var i = 0; i < coloredPixels; i++)
            {
                if (offset >= data.Length)
                {
                    break;
                }

                currentRow.Add(data[offset++]);
            }
        }

        if (currentRow.Count > 0)
        {
            maxWidth = Math.Max(maxWidth, currentRow.Count);
            rows.Add(currentRow.ToArray());
        }

        if (maxWidth == 0 || rows.Count == 0)
        {
            return null;
        }

        var image = new IndexedImage(maxWidth, rows.Count);

        for (var y = 0; y < rows.Count; y++)
        {
            var row = rows[y];

            for (var x = 0; x < row.Length; x++)
            {
                image.SetPixel(x, y, row[x]);
            }
        }

        return image;
    }
}