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
}