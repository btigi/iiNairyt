using System.Buffers.Binary;
using ii.Nairyt.Model;

namespace ii.Nairyt.Decoding;

internal static class TyrianCompositeEncoder
{
    public static byte[] Encode(IReadOnlyList<ShpBank> banks)
    {
        ArgumentNullException.ThrowIfNull(banks);

        if (banks.Count > ushort.MaxValue)
        {
            throw new InvalidDataException("Composite SHP bank count must fit in 16 bits.");
        }

        var encodedBanks = new byte[banks.Count][];

        for (var index = 0; index < banks.Count; index++)
        {
            var bank = banks[index] ?? throw new InvalidDataException("Composite SHP bank is missing.");
            try
            {
                encodedBanks[index] = bank.Format switch
                {
                    ShpBankFormat.SpriteSheet => SpriteSheetEncoder.Encode(bank.Sprites),
                    ShpBankFormat.Estsc => EstscEncoder.Encode(bank.Sprites),
                    _ => throw new InvalidDataException($"Unsupported SHP bank format: {bank.Format}"),
                };
            }
            catch (InvalidDataException exception)
            {
                throw new InvalidDataException($"Composite SHP bank {index}: {exception.Message}", exception);
            }
        }

        var headerSize = sizeof(ushort) + (banks.Count * sizeof(uint));
        var totalSize = headerSize;

        foreach (var encodedBank in encodedBanks)
        {
            totalSize += encodedBank.Length;
        }

        var output = new byte[totalSize];
        BinaryPrimitives.WriteUInt16LittleEndian(output, (ushort)banks.Count);
        var position = headerSize;

        for (var index = 0; index < encodedBanks.Length; index++)
        {
            BinaryPrimitives.WriteUInt32LittleEndian(output.AsSpan(sizeof(ushort) + (index * sizeof(uint))), (uint)position);
            encodedBanks[index].CopyTo(output.AsSpan(position));
            position += encodedBanks[index].Length;
        }

        return output;
    }
}
