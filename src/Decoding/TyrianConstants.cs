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

    public const int HdtItemCountFields = 7;
    public const int HdtHeaderSize = (HdtItemCountFields * sizeof(ushort)) + sizeof(ushort);
    public const int HdtNameFieldSize = 30;
    public const int HdtWeaponPatternRecordSize = 0x50;
    public const int HdtWeaponPortRecordSize = 82;
    public const int HdtSpecialRecordSize = 37;
    public const int HdtPowerRecordSize = 37;
    public const int HdtShipRecordSize = 41;
    public const int HdtExpandedShipRecordSize = 37;
    public const int HdtExpandedShipSectionPadding = 4;
    public const int HdtSidekickRecordSize = 86;
    public const int HdtShieldRecordSize = 37;
    public const int HdtExpandedPortSectionPadding = 29;
    public const int HdtExpandedShipToOptionPadding = 45;
    public const int HdtWeaponPatternFrames = 8;
    public const int HdtWeaponFiringModes = 2;
    public const int HdtWeaponPowerLevels = 11;
    public const int HdtOptionAnimationFrames = 20;
    public const int HdtEnemyAnimationFrames = 20;
    public const int HdtEnemyTurretDirections = 3;

    public const int HdtSharewareWeaponCount = 780;
    public const int HdtSharewareWeaponPortCount = 42;
    public const int HdtSpecialCount = 46;
    public const int HdtSharewarePowerSystemCount = 6;
    public const int HdtSharewareShipCount = 13;
    public const int HdtSharewareOptionCount = 30;
    public const int HdtSharewareShieldCount = 10;
    public const int HdtSharewareEnemyCount = 850;

    public const int PicImageCount = 13;
    public const int PicWidth = 320;
    public const int PicHeight = 200;
    public const byte PicRleRunMarker = 0xC0;
    public const byte PicRleRunLengthMask = 0x3F;

    public static readonly int[] PicPaletteIndices =
    [
        0, 7, 5, 8, 10, 5, 18, 19, 19, 20, 21, 22, 5,
    ];
}