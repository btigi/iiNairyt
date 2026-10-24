using ii.Nairyt.Model;

namespace ii.Nairyt.Decoding;

internal static class EstscEncoder
{
    public static byte[] Encode(IReadOnlyList<ShpSprite?> sprites)
    {
        ArgumentNullException.ThrowIfNull(sprites);

        if (sprites.Count > ushort.MaxValue)
        {
            throw new InvalidDataException("ESTSC sprite count must fit in 16 bits.");
        }

        using var stream = new MemoryStream();
        WriteUInt16(stream, (ushort)sprites.Count);

        foreach (var sprite in sprites)
        {
            if (sprite is null)
            {
                stream.WriteByte(0);
                continue;
            }

            Validate(sprite);
            var compressed = Compress(sprite);

            if (compressed.Length > ushort.MaxValue)
            {
                throw new InvalidDataException("ESTSC sprite compressed data exceeds 65535 bytes.");
            }

            stream.WriteByte(1);
            WriteUInt16(stream, (ushort)sprite.Width);
            WriteUInt16(stream, (ushort)sprite.Height);
            WriteUInt16(stream, (ushort)compressed.Length);
            stream.Write(compressed);
        }

        return stream.ToArray();
    }

    private static void Validate(ShpSprite sprite)
    {
        if (sprite.Width <= 0 || sprite.Height <= 0)
        {
            throw new InvalidDataException("Sprite dimensions must be greater than zero.");
        }

        if (sprite.Width > ushort.MaxValue || sprite.Height > ushort.MaxValue)
        {
            throw new InvalidDataException("ESTSC sprite dimensions must fit in 16 bits.");
        }

        if (sprite.Pixels is null || (long)sprite.Width * sprite.Height != sprite.Pixels.Length)
        {
            throw new InvalidDataException("Sprite pixel buffer does not match its dimensions.");
        }
    }

    private static byte[] Compress(ShpSprite sprite)
    {
        using var stream = new MemoryStream();

        for (var y = 0; y < sprite.Height; y++)
        {
            var row = y * sprite.Width;
            var x = 0;

            while (x < sprite.Width)
            {
                var pixel = sprite.Pixels[row + x];

                if (pixel == 0)
                {
                    var run = 0;

                    while (x + run < sprite.Width && run < byte.MaxValue && sprite.Pixels[row + x + run] == 0)
                    {
                        run++;
                    }

                    stream.WriteByte(TyrianConstants.RleSkipPixels);
                    stream.WriteByte((byte)run);
                    x += run;
                    continue;
                }

                if (pixel >= TyrianConstants.RleTransparentPixel)
                {
                    throw new InvalidDataException($"ESTSC sprites cannot use palette index {pixel}.");
                }

                stream.WriteByte(pixel);
                x++;
            }
        }

        return stream.ToArray();
    }

    private static void WriteUInt16(Stream stream, ushort value)
    {
        stream.WriteByte((byte)value);
        stream.WriteByte((byte)(value >> 8));
    }
}
