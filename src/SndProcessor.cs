using ii.Nairyt.Decoding;
using ii.Nairyt.Model;

namespace ii.Nairyt;

public static class SndProcessor
{
    public const int DefaultSampleRate = TyrianConstants.SndDefaultSampleRate;

    public static IReadOnlyList<ExportedAudio> Read(string sndPath, int sampleRate = DefaultSampleRate, bool isVoiceFile = false)
    {
        ArgumentException.ThrowIfNullOrEmpty(sndPath);

        if (!File.Exists(sndPath))
        {
            throw new FileNotFoundException($"SND file not found: {sndPath}", sndPath);
        }

        if (sampleRate <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(sampleRate), sampleRate, "Sample rate must be positive.");
        }

        var data = File.ReadAllBytes(sndPath);
        var decodedSamples = TyrianSndDecoder.Decode(data, sndPath, isVoiceFile);
        var exports = new List<ExportedAudio>(decodedSamples.Count);

        foreach (var sample in decodedSamples)
        {
            if (sample.PcmData.Length == 0)
            {
                continue;
            }

            exports.Add(new ExportedAudio(sample.Name, WavEncoder.EncodeSigned8BitPcm(sample.PcmData, sampleRate)));
        }

        if (exports.Count == 0)
        {
            throw new InvalidDataException($"No audio samples decoded from {sndPath}");
        }

        return exports;
    }
}