namespace ii.Nairyt.Decoding;

internal static class TyrianSoundNames
{
    public const int SfxCount = 29;
    public const int VoiceCount = 9;

    private static readonly string[] Titles =
    [
        "SCALEDN2",
        "F2",
        "TEMP10",
        "EXPLSM",
        "PASS3",
        "TEMP2",
        "BYPASS1",
        "EXP1RT",
        "EXPLLOW",
        "TEMP13",
        "EXPRETAP",
        "MT2BOOM",
        "TEMP3",
        "LAZB",
        "LAZGUN2",
        "SPRING",
        "WARNING",
        "ITEM",
        "HIT2",
        "MACHNGUN",
        "HYPERD2",
        "EXPLHUG",
        "CLINK1",
        "CLICK",
        "SCALEDN1",
        "TEMP11",
        "TEMP16",
        "SMALL1",
        "POWERUP",
        "VOICE1",
        "VOICE2",
        "VOICE3",
        "VOICE4",
        "VOICE5",
        "VOICE6",
        "VOICE7",
        "VOICE8",
        "VOICE9",
    ];

    public static string GetTitle(int sampleIndex, bool isVoiceFile)
    {
        var titleIndex = isVoiceFile ? SfxCount + sampleIndex : sampleIndex;

        if (titleIndex >= 0 && titleIndex < Titles.Length)
        {
            return Titles[titleIndex];
        }

        return $"sample{sampleIndex + 1}";
    }
}