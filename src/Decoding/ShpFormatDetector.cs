using System.Buffers.Binary;
using ii.Nairyt.Model;

namespace ii.Nairyt.Decoding;

internal static class ShpFormatDetector
{
    public static ShpFormat Detect(string filePath, ReadOnlySpan<byte> data)
    {
        ArgumentException.ThrowIfNullOrEmpty(filePath);

        if (data.Length < sizeof(ushort))
        {
            throw new InvalidDataException("File is too small to be a Tyrian SHP file.");
        }

        if (IsComposite(data))
        {
            return ShpFormat.TyrianComposite;
        }

        if (IsEstsc(data))
        {
            return ShpFormat.Estsc;
        }

        if (IsSpriteSheet(data))
        {
            return ShpFormat.SpriteSheet;
        }

        return ShpFormat.Estsc;
    }

    private static bool IsComposite(ReadOnlySpan<byte> data)
    {
        var sectionCount = BinaryPrimitives.ReadUInt16LittleEndian(data);

        if (sectionCount is 0 or > 32)
        {
            return false;
        }

        var headerSize = sizeof(ushort) + (sectionCount * sizeof(uint));

        if (headerSize > data.Length)
        {
            return false;
        }

        uint previous = 0;

        for (var i = 0; i < sectionCount; i++)
        {
            var position = BinaryPrimitives.ReadUInt32LittleEndian(data[(sizeof(ushort) + (i * sizeof(uint)))..]);

            if (position < (uint)headerSize || position >= (uint)data.Length || position < previous)
            {
                return false;
            }

            previous = position;
        }

        return true;
    }

    private static bool IsEstsc(ReadOnlySpan<byte> data)
    {
        var count = BinaryPrimitives.ReadUInt16LittleEndian(data);
        var offset = sizeof(ushort);

        for (var i = 0; i < count; i++)
        {
            if (offset >= data.Length)
            {
                return false;
            }

            var populated = data[offset++];

            if (populated == 0)
            {
                continue;
            }

            if (offset + (sizeof(ushort) * 3) > data.Length)
            {
                return false;
            }

            var width = BinaryPrimitives.ReadUInt16LittleEndian(data[offset..]);
            var height = BinaryPrimitives.ReadUInt16LittleEndian(data[(offset + sizeof(ushort))..]);
            var compressedSize = BinaryPrimitives.ReadUInt16LittleEndian(data[(offset + (sizeof(ushort) * 2))..]);
            offset += sizeof(ushort) * 3;

            if (width == 0 || height == 0 || offset + compressedSize > data.Length)
            {
                return false;
            }

            offset += compressedSize;
        }

        return offset == data.Length;
    }

    private static bool IsSpriteSheet(ReadOnlySpan<byte> data)
    {
        var tableSize = BinaryPrimitives.ReadUInt16LittleEndian(data);

        if (tableSize < sizeof(ushort) || tableSize % sizeof(ushort) != 0 || tableSize > data.Length)
        {
            return false;
        }

        var count = tableSize / sizeof(ushort);

        for (var i = 0; i < count; i++)
        {
            var spriteOffset = BinaryPrimitives.ReadUInt16LittleEndian(data[(i * sizeof(ushort))..]);

            if (spriteOffset < tableSize || spriteOffset >= data.Length || !ContainsTerminator(data[spriteOffset..]))
            {
                return false;
            }
        }

        return true;
    }

    private static bool ContainsTerminator(ReadOnlySpan<byte> sprite)
    {
        foreach (var value in sprite)
        {
            if (value == TyrianConstants.SpriteRowTerminator)
            {
                return true;
            }
        }

        return false;
    }
}
