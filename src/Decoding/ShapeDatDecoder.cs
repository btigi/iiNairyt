using ii.Nairyt.Imaging;

namespace ii.Nairyt.Decoding;

internal static class ShapeDatDecoder
{
    public static DecodedShapeDat Decode(ReadOnlySpan<byte> data)
    {
        var bytes = data.ToArray();
        var reader = new BinaryDataReader(bytes);
        var width = TyrianConstants.ShapeColumns * TyrianConstants.LevelTileWidth;
        var height = TyrianConstants.ShapeRows * TyrianConstants.LevelTileHeight;
        var sheet = new IndexedImage(width, height);
        var tileWidth = TyrianConstants.LevelTileWidth;
        var tileLength = tileWidth * TyrianConstants.LevelTileHeight;

        for (var tileIndex = 0; tileIndex < TyrianConstants.ShapeTileCount; tileIndex++)
        {
            if (reader.ReadByte() != 0)
            {
                continue;
            }

            var tilePixels = reader.ReadBytes(tileLength);
            var column = tileIndex % TyrianConstants.ShapeColumns;
            var row = tileIndex / TyrianConstants.ShapeColumns;
            var destX = column * tileWidth;
            var destY = row * TyrianConstants.LevelTileHeight;

            for (var y = 0; y < TyrianConstants.LevelTileHeight; y++)
            {
                var destination = ((destY + y) * width) + destX;
                tilePixels.Slice(y * tileWidth, tileWidth).CopyTo(sheet.Pixels.AsSpan(destination, tileWidth));
            }
        }

        var trailer = reader.Position == bytes.Length ? [] : bytes.AsSpan(reader.Position).ToArray();

        return new DecodedShapeDat(sheet, trailer);
    }
}

internal sealed record DecodedShapeDat(IndexedImage Sheet, byte[] Trailer);
