namespace ii.Nairyt.Decoding;

internal class TyrianLevelDecoder
{
    private readonly byte[] _data;
    private readonly uint[] _mapPositions;
    private BinaryDataReader _reader;

    public TyrianLevelDecoder(byte[] data)
    {
        _data = data;
        _reader = new BinaryDataReader(data);
        var positionCount = _reader.ReadUInt16();
        _mapPositions = new uint[positionCount];

        for (var i = 0; i < positionCount; i++)
        {
            _mapPositions[i] = _reader.ReadUInt32();
        }
    }

    public int MapCount => _mapPositions.Length / 2;

    public DecodedTyrianMap ReadMap(int mapIndex)
    {
        if (mapIndex < 1 || mapIndex > MapCount)
        {
            throw new ArgumentOutOfRangeException(nameof(mapIndex));
        }

        _reader.Seek((int)_mapPositions[(mapIndex - 1) * 2]);

        var mapFileId = (char)_reader.ReadByte();
        var shapeFileId = (char)_reader.ReadByte();
        _reader.ReadUInt16();
        _reader.ReadUInt16();
        _reader.ReadUInt16();
        var levelEnemyCount = _reader.ReadUInt16();

        for (var i = 0; i < levelEnemyCount; i++)
        {
            _reader.ReadUInt16();
        }

        var eventCount = _reader.ReadUInt16();

        for (var i = 0; i < eventCount; i++)
        {
            _reader.ReadBytes(11);
        }

        var shapeTables = new ushort[TyrianConstants.ShapeTableCount][];

        for (var tableIndex = 0; tableIndex < TyrianConstants.ShapeTableCount; tableIndex++)
        {
            shapeTables[tableIndex] = new ushort[TyrianConstants.ShapeTableLength];

            for (var entryIndex = 0; entryIndex < TyrianConstants.ShapeTableLength; entryIndex++)
            {
                shapeTables[tableIndex][entryIndex] = _reader.ReadBigEndianUInt16();
            }
        }

        var layers = new List<int>[TyrianConstants.ShapeTableCount];
        layers[0] = DecodeLayer(
            _reader.ReadBytes(TyrianConstants.Layer1CompressedColumns * TyrianConstants.Layer1CompressedRows),
            shapeTables[0],
            TyrianConstants.Layer1CompressedRows,
            TyrianConstants.Layer1CompressedColumns);

        layers[1] = DecodeLayer(
            _reader.ReadBytes(TyrianConstants.Layer2CompressedColumns * TyrianConstants.Layer2CompressedRows),
            shapeTables[1],
            TyrianConstants.Layer2CompressedRows,
            TyrianConstants.Layer2CompressedColumns);

        layers[2] = DecodeLayer(
            _reader.ReadBytes(TyrianConstants.Layer3CompressedColumns * TyrianConstants.Layer3CompressedRows),
            shapeTables[2],
            TyrianConstants.Layer3CompressedRows,
            TyrianConstants.Layer3CompressedColumns);

        return new DecodedTyrianMap(mapFileId, shapeFileId, layers);
    }

    private static List<int> DecodeLayer(
        ReadOnlySpan<byte> compressedMap,
        ushort[] shapeTable,
        int compressedRows,
        int compressedColumns)
    {
        var mapData = new List<int>(TyrianConstants.LevelMapWidth * TyrianConstants.LevelMapHeight);

        for (var i = 0; i < TyrianConstants.LevelMapWidth * TyrianConstants.LevelMapHeight; i++)
        {
            mapData.Add(0);
        }

        var bufferIndex = 0;

        for (var y = 0; y < compressedRows; y++)
        {
            for (var x = 0; x < compressedColumns; x++)
            {
                var shapeIndex = compressedMap[bufferIndex++];
                mapData[(y * TyrianConstants.LevelMapWidth) + x] = shapeTable[shapeIndex];
            }
        }

        return mapData;
    }
}

internal record DecodedTyrianMap(char MapFileId, char ShapeFileId, IReadOnlyList<int>[] Layers)
{
    public string ShapeDatFileName => $"shapes{char.ToLowerInvariant(ShapeFileId)}.dat";

    public string TilesetBaseName => Path.GetFileNameWithoutExtension(ShapeDatFileName);
}
