using System.Buffers.Binary;
using ii.Nairyt.Model;

namespace ii.Nairyt.Decoding;

internal static class SpriteSheetEncoder
{
    public static byte[] Encode(IReadOnlyList<ShpSprite?> sprites)
    {
        ArgumentNullException.ThrowIfNull(sprites);

        if (sprites.Count > ushort.MaxValue / sizeof(ushort))
        {
            throw new InvalidDataException("Sprite sheet has too many sprites for a 16-bit offset table.");
        }

        if (sprites.Count == 0)
        {
            return [0, 0];
        }

        using var spriteData = new MemoryStream();
        var relativeOffsets = new int[sprites.Count];
        int? emptyOffset = null;

        for (var index = 0; index < sprites.Count; index++)
        {
            var sprite = sprites[index];

            if (sprite is null)
            {
                emptyOffset ??= (int)spriteData.Length;

                if (emptyOffset == spriteData.Length)
                {
                    spriteData.WriteByte(TyrianConstants.SpriteRowTerminator);
                }

                relativeOffsets[index] = emptyOffset.Value;
                continue;
            }

            ValidateSprite(sprite, index);
            relativeOffsets[index] = (int)spriteData.Length;
            WriteSprite(spriteData, sprite);
        }

        var tableSize = sprites.Count * sizeof(ushort);

        if ((long)tableSize + spriteData.Length > ushort.MaxValue)
        {
            throw new InvalidDataException("Sprite sheet exceeds 65535 bytes.");
        }

        var encodedSprites = spriteData.ToArray();
        var output = new byte[tableSize + encodedSprites.Length];

        for (var index = 0; index < sprites.Count; index++)
        {
            var offset = tableSize + relativeOffsets[index];
            BinaryPrimitives.WriteUInt16LittleEndian(output.AsSpan(index * sizeof(ushort)), (ushort)offset);
        }

        encodedSprites.CopyTo(output.AsSpan(tableSize));
        return output;
    }

    private static void ValidateSprite(ShpSprite sprite, int index)
    {
        if (sprite.Width <= 0 || sprite.Height <= 0)
        {
            throw new InvalidDataException($"Sprite sheet sprite {index} has empty dimensions.");
        }

        if (sprite.Pixels is null || (long)sprite.Width * sprite.Height != sprite.Pixels.Length)
        {
            throw new InvalidDataException($"Sprite sheet sprite {index} pixel buffer does not match its dimensions.");
        }
    }

    private static void WriteSprite(Stream stream, ShpSprite sprite)
    {
        for (var y = 0; y < sprite.Height; y++)
        {
            var row = y * sprite.Width;
            var x = 0;
            var endedOnOpaque = true;

            while (x < sprite.Width)
            {
                var remaining = sprite.Width - x;

                if (RemainderIsTransparent(sprite.Pixels, row + x, remaining))
                {
                    // 0x0F ends the whole sprite, so a transparent tail of 15 or more is stored as literal zeros.
                    if (remaining < TyrianConstants.SpriteRowTerminator)
                    {
                        stream.WriteByte((byte)remaining);
                        endedOnOpaque = false;
                        break;
                    }

                    var literalZeros = Math.Min(15, remaining);
                    stream.WriteByte((byte)(literalZeros << 4));

                    for (var i = 0; i < literalZeros; i++)
                    {
                        stream.WriteByte(0);
                    }

                    x += literalZeros;
                    continue;
                }

                var transparent = 0;

                while (x + transparent < sprite.Width && transparent < 15 && sprite.Pixels[row + x + transparent] == 0)
                {
                    transparent++;
                }

                var opaqueStart = x + transparent;
                var opaque = 0;

                while (opaque < 15 && opaqueStart + opaque < sprite.Width && sprite.Pixels[row + opaqueStart + opaque] != 0)
                {
                    opaque++;
                }

                if (opaque == 0)
                {
                    stream.WriteByte((byte)(transparent << 4));

                    for (var i = 0; i < transparent; i++)
                    {
                        stream.WriteByte(0);
                    }

                    x += transparent;
                    continue;
                }

                stream.WriteByte((byte)((opaque << 4) | transparent));

                for (var i = 0; i < opaque; i++)
                {
                    stream.WriteByte(sprite.Pixels[row + opaqueStart + i]);
                }

                x = opaqueStart + opaque;
            }

            if (endedOnOpaque)
            {
                stream.WriteByte(0);
            }
        }

        stream.WriteByte(TyrianConstants.SpriteRowTerminator);
    }

    private static bool RemainderIsTransparent(byte[] pixels, int start, int length)
    {
        for (var i = 0; i < length; i++)
        {
            if (pixels[start + i] != 0)
            {
                return false;
            }
        }

        return true;
    }
}
