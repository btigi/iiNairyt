using ii.Nairyt.Imaging;

namespace ii.Nairyt.Decoding;

internal static class ShapeDatDecoder
{
    public static IndexedImage LoadTileset(string shapeDatPath)
    {
        var data = File.ReadAllBytes(shapeDatPath);
        var reader = new BinaryDataReader(data);
        var sheet = new IndexedImage(TyrianConstants.ShapeColumns * TyrianConstants.LevelTileWidth, TyrianConstants.ShapeRows * TyrianConstants.LevelTileHeight);

        for (var tileIndex = 0; tileIndex < TyrianConstants.ShapeTileCount; tileIndex++)
        {
            var isBlank = reader.ReadByte() != 0;
            var tilePixels = isBlank
                ? new byte[TyrianConstants.LevelTileWidth * TyrianConstants.LevelTileHeight]
                : reader.ReadBytes(TyrianConstants.LevelTileWidth * TyrianConstants.LevelTileHeight).ToArray();

            var column = tileIndex % TyrianConstants.ShapeColumns;
            var row = tileIndex / TyrianConstants.ShapeColumns;
            var destX = column * TyrianConstants.LevelTileWidth;
            var destY = row * TyrianConstants.LevelTileHeight;

            for (var y = 0; y < TyrianConstants.LevelTileHeight; y++)
            {
                for (var x = 0; x < TyrianConstants.LevelTileWidth; x++)
                {
                    var pixel = tilePixels[(y * TyrianConstants.LevelTileWidth) + x];
                    sheet.SetPixel(destX + x, destY + y, pixel);
                }
            }
        }

        return sheet;
    }
}