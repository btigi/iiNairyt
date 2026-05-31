using System.Buffers.Binary;
using System.Text;

namespace ii.Nairyt.Decoding;

internal static class WavEncoder
{
    private const short PcmFormat = 1;
    private const short BitsPerSample = 16;
    private const short Channels = 1;
    private const int FmtChunkSize = 16;
    private const int HeaderSize = 44;

    public static byte[] EncodeSigned8BitPcm(ReadOnlySpan<byte> pcmData, int sampleRate)
    {
        if (pcmData.IsEmpty)
        {
            throw new InvalidDataException("SND file contains no audio data.");
        }

        var dataChunkSize = pcmData.Length * sizeof(short);
        var wavData = new byte[HeaderSize + dataChunkSize];
        WriteHeader(wavData, sampleRate, dataChunkSize);
        WriteSamples(wavData.AsSpan(HeaderSize), pcmData);
        return wavData;
    }

    private static void WriteHeader(Span<byte> destination, int sampleRate, int dataChunkSize)
    {
        var byteRate = sampleRate * Channels * BitsPerSample / 8;
        var blockAlign = (short)(Channels * BitsPerSample / 8);
        var riffChunkSize = HeaderSize + dataChunkSize - 8;

        Encoding.ASCII.GetBytes("RIFF").CopyTo(destination);
        BinaryPrimitives.WriteInt32LittleEndian(destination[4..], riffChunkSize);
        Encoding.ASCII.GetBytes("WAVE").CopyTo(destination[8..]);

        Encoding.ASCII.GetBytes("fmt ").CopyTo(destination[12..]);
        BinaryPrimitives.WriteInt32LittleEndian(destination[16..], FmtChunkSize);
        BinaryPrimitives.WriteInt16LittleEndian(destination[20..], PcmFormat);
        BinaryPrimitives.WriteInt16LittleEndian(destination[22..], Channels);
        BinaryPrimitives.WriteInt32LittleEndian(destination[24..], sampleRate);
        BinaryPrimitives.WriteInt32LittleEndian(destination[28..], byteRate);
        BinaryPrimitives.WriteInt16LittleEndian(destination[32..], blockAlign);
        BinaryPrimitives.WriteInt16LittleEndian(destination[34..], BitsPerSample);

        Encoding.ASCII.GetBytes("data").CopyTo(destination[36..]);
        BinaryPrimitives.WriteInt32LittleEndian(destination[40..], dataChunkSize);
    }

    private static void WriteSamples(Span<byte> destination, ReadOnlySpan<byte> pcmData)
    {
        for (var i = 0; i < pcmData.Length; i++)
        {
            var sample = (short)((sbyte)pcmData[i] << 8);
            BinaryPrimitives.WriteInt16LittleEndian(destination[(i * sizeof(short))..], sample);
        }
    }
}