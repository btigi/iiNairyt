using System.Buffers.Binary;

namespace ii.Nairyt.Decoding;

internal readonly record struct DecodedSoundSample(string Name, byte[] PcmData);

internal static class TyrianSndDecoder
{
    public static IReadOnlyList<DecodedSoundSample> Decode(byte[] data, string filePath, bool isVoiceFile)
    {
        var fileName = Path.GetFileName(filePath);
        return DecodeContainer(data, fileName, isVoiceFile);
    }

    private static IReadOnlyList<DecodedSoundSample> DecodeContainer(byte[] data, string fileName, bool isVoiceFile)
    {
        if (!TryDecodeContainer(data, fileName, isVoiceFile, out var samples))
        {
            throw new InvalidDataException($"Failed to decode Tyrian SND container: {fileName}");
        }

        return samples;
    }

    private static bool TryDecodeContainer(byte[] data, string fileName, bool isVoiceFile, out IReadOnlyList<DecodedSoundSample> samples)
    {
        samples = [];

        if (data.Length < sizeof(ushort) + sizeof(uint))
        {
            return false;
        }

        var sampleCount = BinaryPrimitives.ReadUInt16LittleEndian(data);
        var headerSize = sizeof(ushort) + (sampleCount * sizeof(uint));

        if (sampleCount == 0 || headerSize > data.Length)
        {
            return false;
        }

        var positions = new int[sampleCount + 1];

        for (var i = 0; i < sampleCount; i++)
        {
            positions[i] = (int)BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(sizeof(ushort) + (i * sizeof(uint))));
        }

        positions[sampleCount] = data.Length;

        if (!ArePositionsValid(positions, data.Length, headerSize))
        {
            return false;
        }

        var baseName = Path.GetFileNameWithoutExtension(fileName);
        var decodedSamples = new List<DecodedSoundSample>(sampleCount);

        for (var i = 0; i < sampleCount; i++)
        {
            var sampleLength = positions[i + 1] - positions[i];

            if (isVoiceFile)
            {
                sampleLength = sampleLength >= TyrianConstants.VoiceSampleTrimBytes ? sampleLength - TyrianConstants.VoiceSampleTrimBytes : 0;
            }

            if (sampleLength <= 0)
            {
                continue;
            }

            if (sampleLength > TyrianConstants.SndMaxSampleBytes)
            {
                throw new InvalidDataException($"Sample {i + 1} in {fileName} exceeds the maximum supported size of {TyrianConstants.SndMaxSampleBytes} bytes.");
            }

            var pcmData = data.AsSpan(positions[i], sampleLength).ToArray();
            var title = TyrianSoundNames.GetTitle(i, isVoiceFile);
            decodedSamples.Add(new DecodedSoundSample($"{baseName}-{i + 1:D2}-{title}", pcmData));
        }

        samples = decodedSamples;
        return decodedSamples.Count > 0;
    }

    private static bool ArePositionsValid(int[] positions, int fileLength, int headerSize)
    {
        for (var i = 0; i < positions.Length - 1; i++)
        {
            if (positions[i] < headerSize || positions[i] > fileLength)
            {
                return false;
            }

            if (positions[i] > positions[i + 1])
            {
                return false;
            }
        }

        return positions[^1] == fileLength;
    }
}