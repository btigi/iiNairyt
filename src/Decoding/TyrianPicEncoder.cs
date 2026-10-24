using System.Buffers.Binary;

namespace ii.Nairyt.Decoding;

internal static class TyrianPicEncoder
{
    public static byte[] Encode(IReadOnlyList<byte[]> images)
    {
        ArgumentNullException.ThrowIfNull(images);

        if (images.Count != TyrianConstants.PicImageCount)
        {
            throw new InvalidDataException($"PIC files contain {TyrianConstants.PicImageCount} images.");
        }

        var pixelCount = TyrianConstants.PicWidth * TyrianConstants.PicHeight;
        var compressed = new byte[images.Count][];

        for (var imageIndex = 0; imageIndex < images.Count; imageIndex++)
        {
            var pixels = images[imageIndex] ?? throw new InvalidDataException($"PIC image {imageIndex} is missing.");

            if (pixels.Length != pixelCount)
            {
                throw new InvalidDataException($"PIC image {imageIndex} must contain {pixelCount} pixels.");
            }

            compressed[imageIndex] = EncodeRle(pixels);
        }

        var headerSize = sizeof(ushort) + (images.Count * sizeof(int));
        var payloadSize = 0;

        foreach (var image in compressed)
        {
            payloadSize += image.Length;
        }

        using var stream = new MemoryStream(headerSize + payloadSize);
        WriteUInt16(stream, (ushort)TyrianConstants.PicImageCount);

        var offset = headerSize;

        foreach (var image in compressed)
        {
            WriteInt32(stream, offset);
            offset += image.Length;
        }

        foreach (var image in compressed)
        {
            stream.Write(image);
        }

        return stream.ToArray();
    }

    private static byte[] EncodeRle(ReadOnlySpan<byte> pixels)
    {
        using var stream = new MemoryStream();
        var index = 0;

        while (index < pixels.Length)
        {
            var color = pixels[index];
            var run = 1;
            var maxRun = Math.Min(TyrianConstants.PicRleRunLengthMask, pixels.Length - index);

            while (run < maxRun && pixels[index + run] == color)
            {
                run++;
            }

            if (run >= 2 || (color & TyrianConstants.PicRleRunMarker) == TyrianConstants.PicRleRunMarker)
            {
                stream.WriteByte((byte)(TyrianConstants.PicRleRunMarker | run));
                stream.WriteByte(color);
                index += run;
            }
            else
            {
                stream.WriteByte(color);
                index++;
            }
        }

        return stream.ToArray();
    }

    private static void WriteUInt16(Stream stream, ushort value)
    {
        Span<byte> buffer = stackalloc byte[sizeof(ushort)];
        BinaryPrimitives.WriteUInt16LittleEndian(buffer, value);
        stream.Write(buffer);
    }

    private static void WriteInt32(Stream stream, int value)
    {
        Span<byte> buffer = stackalloc byte[sizeof(int)];
        BinaryPrimitives.WriteInt32LittleEndian(buffer, value);
        stream.Write(buffer);
    }
}
