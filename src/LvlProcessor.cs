using ii.Nairyt.Decoding;
using ii.Nairyt.Model;
using SixLabors.ImageSharp.PixelFormats;

namespace ii.Nairyt;

public static class LvlProcessor
{
    public static IReadOnlyList<ExportedLevel> Read(
        string lvlPath,
        string palettePath,
        IReadOnlyList<string> datFiles)
    {
        ArgumentException.ThrowIfNullOrEmpty(lvlPath);
        ArgumentException.ThrowIfNullOrEmpty(palettePath);
        ArgumentNullException.ThrowIfNull(datFiles);

        if (!File.Exists(lvlPath))
        {
            throw new FileNotFoundException($"LVL file not found: {lvlPath}", lvlPath);
        }

        if (!File.Exists(palettePath))
        {
            throw new FileNotFoundException($"Palette file not found: {palettePath}", palettePath);
        }

        var data = File.ReadAllBytes(lvlPath);
        var baseName = Path.GetFileNameWithoutExtension(lvlPath);
        var datFileIndex = IndexDatFiles(datFiles);
        var palette = TyrianPalette.Load(palettePath);
        var decoder = new TyrianLevelDecoder(data);
        var exports = new List<ExportedLevel>(decoder.MapCount);
        var tilesetCache = new Dictionary<string, ExportedImage>(StringComparer.OrdinalIgnoreCase);

        for (var mapIndex = 1; mapIndex <= decoder.MapCount; mapIndex++)
        {
            var map = decoder.ReadMap(mapIndex);
            var tileset = LoadTileset(map, datFileIndex, palette, tilesetCache);
            var tilesetImageFileName = $"{map.TilesetBaseName}.png";
            var tmx = TmxBuilder.Build(
                map.Layers,
                map.TilesetBaseName,
                tilesetImageFileName,
                TyrianConstants.ShapeColumns * TyrianConstants.LevelTileWidth,
                TyrianConstants.ShapeRows * TyrianConstants.LevelTileHeight);

            exports.Add(new ExportedLevel($"{baseName}-{mapIndex}", tmx)
            {
                Tileset = tileset,
            });
        }

        return exports;
    }

    private static ExportedImage? LoadTileset(
        DecodedTyrianMap map,
        IReadOnlyDictionary<string, string> datFiles,
        Rgba32[] palette,
        Dictionary<string, ExportedImage> tilesetCache)
    {
        if (tilesetCache.TryGetValue(map.TilesetBaseName, out var cachedTileset))
        {
            return cachedTileset;
        }

        if (!datFiles.TryGetValue(map.ShapeDatFileName, out var shapeDatPath))
        {
            return null;
        }

        var tilesetImage = ShapeDatDecoder.LoadTileset(shapeDatPath).ToRgba32(palette);
        var exportedTileset = new ExportedImage(map.TilesetBaseName, tilesetImage);
        tilesetCache[map.TilesetBaseName] = exportedTileset;
        return exportedTileset;
    }

    private static Dictionary<string, string> IndexDatFiles(IReadOnlyList<string> datFiles)
    {
        var index = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var datFile in datFiles)
        {
            if (string.IsNullOrWhiteSpace(datFile))
            {
                continue;
            }

            var fullPath = Path.GetFullPath(datFile);

            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException($"DAT file not found: {fullPath}", fullPath);
            }

            index[Path.GetFileName(fullPath)] = fullPath;
        }

        return index;
    }
}