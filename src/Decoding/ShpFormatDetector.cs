using System.Buffers.Binary;
using ii.Nairyt.Model;

namespace ii.Nairyt.Decoding;

internal static class ShpFormatDetector
{
    public static ShpFormat Detect(string filePath, ReadOnlySpan<byte> data)
    {
        var fileName = Path.GetFileName(filePath);

        if (data.Length < sizeof(ushort))
        {
            throw new InvalidDataException("File is too small to be a Tyrian SHP file.");
        }

        var headerValue = BinaryPrimitives.ReadUInt16LittleEndian(data);

        if (headerValue is > 0 and <= 32 && HasValidCompositeSections(data, headerValue))
        {
            return ShpFormat.TyrianComposite;
        }

        if (headerValue >= 2 && headerValue <= data.Length && headerValue % 2 == 0)
        {
            return ShpFormat.SpriteSheet;
        }

        return ShpFormat.Estsc;
    }

    private static bool HasValidCompositeSections(ReadOnlySpan<byte> data, ushort sectionCount)
    {
        var headerSize = sizeof(ushort) + sectionCount * sizeof(uint);

        if (headerSize > data.Length)
        {
            return false;
        }

        for (var i = 0; i < sectionCount; i++)
        {
            var offset = sizeof(ushort) + i * sizeof(uint);
            var position = BinaryPrimitives.ReadUInt32LittleEndian(data[offset..]);

            if (position >= (uint)data.Length)
            {
                return false;
            }
        }

        return true;
    }
}
