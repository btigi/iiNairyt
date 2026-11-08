using System.Buffers.Binary;
using ii.Nairyt.Model;

namespace ii.Nairyt.Decoding;

internal static class TyrianSndEncoder
{
    private const short PcmFormat = 1;
    private const int WavChunkHeaderSize = 8;
    private const int FmtChunkSize = 16;

    public static byte[] Encode(IReadOnlyList<ExportedAudio> samples, bool isVoiceFile)
    {
        ArgumentNullException.ThrowIfNull(samples);

        if (samples.Count == 0 || samples.Count > ushort.MaxValue)
        {
            throw new InvalidDataException($"SND files must contain between 1 and {ushort.MaxValue} samples.");
        }

        var pcmSamples = new byte[samples.Count][];
        var storedLength = 0;

        for (var i = 0; i < samples.Count; i++)
        {
            var sample = samples[i] ?? throw new InvalidDataException($"SND sample {i + 1} is missing.");
            var pcm = ToSigned8Bit(sample.WavData, i + 1);
            pcmSamples[i] = pcm;
            storedLength += pcm.Length;

            if (isVoiceFile)
            {
                storedLength += TyrianConstants.VoiceSampleTrimBytes;
            }
        }

        var headerSize = sizeof(ushort) + (samples.Count * sizeof(uint));
        var output = new byte[headerSize + storedLength];
        BinaryPrimitives.WriteUInt16LittleEndian(output, (ushort)samples.Count);

        var offset = headerSize;

        for (var i = 0; i < pcmSamples.Length; i++)
        {
            BinaryPrimitives.WriteUInt32LittleEndian(output.AsSpan(sizeof(ushort) + (i * sizeof(uint))), (uint)offset);
            pcmSamples[i].CopyTo(output.AsSpan(offset));
            offset += pcmSamples[i].Length;

            if (isVoiceFile)
            {
                offset += TyrianConstants.VoiceSampleTrimBytes;
            }
        }

        return output;
    }

    private static byte[] ToSigned8Bit(byte[] wav, int sampleNumber)
    {
        if (wav is null || wav.Length < 12)
        {
            throw new InvalidDataException($"WAV sample {sampleNumber} is not a RIFF waveform.");
        }

        if (!HasId(wav, 0, "RIFF"u8) || !HasId(wav, 8, "WAVE"u8))
        {
            throw new InvalidDataException($"WAV sample {sampleNumber} is not a RIFF waveform.");
        }

        short? format = null;
        short? channels = null;
        int? sampleRate = null;
        short? bitsPerSample = null;
        var dataOffset = -1;
        var dataLength = 0;
        var offset = 12;

        while (offset + WavChunkHeaderSize <= wav.Length)
        {
            var chunkSize = BinaryPrimitives.ReadInt32LittleEndian(wav.AsSpan(offset + 4));

            if (chunkSize < 0 || (long)offset + WavChunkHeaderSize + chunkSize > wav.Length)
            {
                throw new InvalidDataException($"WAV sample {sampleNumber} is truncated.");
            }

            var chunkStart = offset + WavChunkHeaderSize;

            if (HasId(wav, offset, "fmt "u8) && format is null)
            {
                if (chunkSize < FmtChunkSize)
                {
                    throw new InvalidDataException($"WAV sample {sampleNumber} has a truncated format chunk.");
                }

                format = BinaryPrimitives.ReadInt16LittleEndian(wav.AsSpan(chunkStart));
                channels = BinaryPrimitives.ReadInt16LittleEndian(wav.AsSpan(chunkStart + 2));
                sampleRate = BinaryPrimitives.ReadInt32LittleEndian(wav.AsSpan(chunkStart + 4));
                bitsPerSample = BinaryPrimitives.ReadInt16LittleEndian(wav.AsSpan(chunkStart + 14));
            }
            else if (HasId(wav, offset, "data"u8) && dataOffset < 0)
            {
                dataOffset = chunkStart;
                dataLength = chunkSize;
            }

            offset = chunkStart + chunkSize;

            if ((chunkSize & 1) != 0 && offset < wav.Length)
            {
                offset++;
            }
        }

        if (format is null || dataOffset < 0)
        {
            throw new InvalidDataException($"WAV sample {sampleNumber} is missing PCM data.");
        }

        if (format != PcmFormat)
        {
            throw new InvalidDataException($"WAV sample {sampleNumber} must be PCM.");
        }

        if (channels != 1)
        {
            throw new InvalidDataException($"WAV sample {sampleNumber} must be mono.");
        }

        if (sampleRate != TyrianConstants.SndDefaultSampleRate)
        {
            throw new InvalidDataException($"WAV sample {sampleNumber} must be {TyrianConstants.SndDefaultSampleRate} Hz.");
        }

        if (bitsPerSample == 16)
        {
            if ((dataLength & 1) != 0)
            {
                throw new InvalidDataException($"WAV sample {sampleNumber} has a truncated sample.");
            }

            var sampleCount = dataLength / sizeof(short);

            if (sampleCount == 0)
            {
                throw new InvalidDataException($"WAV sample {sampleNumber} contains no audio data.");
            }

            if (sampleCount > TyrianConstants.SndMaxSampleBytes)
            {
                throw new InvalidDataException($"WAV sample {sampleNumber} exceeds the maximum supported size of {TyrianConstants.SndMaxSampleBytes} bytes.");
            }

            var pcm = new byte[sampleCount];

            for (var i = 0; i < sampleCount; i++)
            {
                var sample = BinaryPrimitives.ReadInt16LittleEndian(wav.AsSpan(dataOffset + (i * sizeof(short))));
                pcm[i] = (byte)(sample >> 8);
            }

            return pcm;
        }

        if (bitsPerSample == 8)
        {
            if (dataLength == 0)
            {
                throw new InvalidDataException($"WAV sample {sampleNumber} contains no audio data.");
            }

            if (dataLength > TyrianConstants.SndMaxSampleBytes)
            {
                throw new InvalidDataException($"WAV sample {sampleNumber} exceeds the maximum supported size of {TyrianConstants.SndMaxSampleBytes} bytes.");
            }

            var pcm = new byte[dataLength];

            for (var i = 0; i < dataLength; i++)
            {
                pcm[i] = (byte)(wav[dataOffset + i] - 128);
            }

            return pcm;
        }

        throw new InvalidDataException($"WAV sample {sampleNumber} must be 8-bit or 16-bit PCM.");
    }

    private static bool HasId(byte[] wav, int offset, ReadOnlySpan<byte> id)
    {
        return wav.AsSpan(offset, 4).SequenceEqual(id);
    }
}