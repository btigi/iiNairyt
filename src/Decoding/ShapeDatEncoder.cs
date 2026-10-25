using ii.Nairyt.Model;

namespace ii.Nairyt.Decoding;

internal static class ShapeDatEncoder
{
    public static byte[] Encode(ShapeDatData data)
    {
        EnsureSheet(data);

        var tileWidth = TyrianConstants.LevelTileWidth;
        var tileHeight = TyrianConstants.LevelTileHeight;
        var tileLength = tileWidth * tileHeight;
        var capacity = TyrianConstants.ShapeTileCount + (TyrianConstants.ShapeTileCount * tileLength) + data.Trailer.Length;

        using var stream = new MemoryStream(capacity);
        var tile = new byte[tileLength];

        for (var tileIndex = 0; tileIndex < TyrianConstants.ShapeTileCount; tileIndex++)
        {
            var column = tileIndex % TyrianConstants.ShapeColumns;
            var row = tileIndex / TyrianConstants.ShapeColumns;
            var destX = column * tileWidth;
            var destY = row * tileHeight;
            var blank = true;

            for (var y = 0; y < tileHeight; y++)
            {
                var source = ((destY + y) * data.Width) + destX;
                var destination = y * tileWidth;

                for (var x = 0; x < tileWidth; x++)
                {
                    var pixel = data.Pixels[source + x];
                    tile[destination + x] = pixel;

                    if (pixel != 0)
                    {
                        blank = false;
                    }
                }
            }

            if (blank)
            {
                stream.WriteByte(1);
                continue;
            }

            stream.WriteByte(0);
            stream.Write(tile);
        }

        stream.Write(data.Trailer);
        return stream.ToArray();
    }

    internal static void EnsureSheet(ShapeDatData data)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(data.Pixels);
        ArgumentNullException.ThrowIfNull(data.Trailer);

        var width = TyrianConstants.ShapeColumns * TyrianConstants.LevelTileWidth;
        var height = TyrianConstants.ShapeRows * TyrianConstants.LevelTileHeight;

        if (data.Width != width || data.Height != height || data.Pixels.Length != width * height)
        {
            throw new InvalidDataException($"Shape DAT sheet must be {width}x{height}.");
        }
    }
}