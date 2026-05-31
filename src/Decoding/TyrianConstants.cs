namespace ii.Nairyt.Decoding;

internal static class TyrianConstants
{
    public const int PaletteColorCount = 256;
    public const int PaletteEntrySize = 3;
    public const int PaletteSize = PaletteColorCount * PaletteEntrySize;
    public const int PaletteMaxComponent = 63;
    public const byte SpriteRowTerminator = 0x0F;

    public const byte RleSkipPixels = 255;
    public const byte RleNextRow = 254;
    public const byte RleTransparentPixel = 253;

    public const int EstscSectionCount = 7;
    public const int SpriteSheetSectionStart = 7;
    public const int SpriteSheetSectionEnd = 12;
    public const int PortraitSectionIndex = 4;

    public const int LevelMapWidth = 15;
    public const int LevelMapHeight = 600;
    public const int LevelTileWidth = 24;
    public const int LevelTileHeight = 28;
    public const int ShapeTableCount = 3;
    public const int ShapeTableLength = 128;
    public const int ShapeTileCount = 600;
    public const int ShapeColumns = 10;
    public const int ShapeRows = 60;
    public const int Layer1CompressedColumns = 14;
    public const int Layer1CompressedRows = 300;
    public const int Layer2CompressedColumns = 14;
    public const int Layer2CompressedRows = 600;
    public const int Layer3CompressedColumns = 15;
    public const int Layer3CompressedRows = 600;

    public const int SndDefaultSampleRate = 11025;
    public const int VoiceSampleTrimBytes = 100;
    public const int SndMaxSampleBytes = ushort.MaxValue;
}