using System.Text;

namespace ii.Nairyt.Decoding;

internal static class EncryptedTextDecoder
{
    private static readonly byte[] CryptKey = [204, 129, 63, 255, 71, 19, 25, 62, 1, 99];

    static EncryptedTextDecoder()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    public static string ReadAllText(byte[] data)
    {
        var reader = new BinaryDataReader(data);
        var builder = new StringBuilder();

        while (reader.Position < data.Length)
        {
            if (builder.Length > 0)
            {
                builder.AppendLine();
            }

            builder.Append(ReadEncryptedPascalString(reader));
        }

        return builder.ToString();
    }

    private static string ReadEncryptedPascalString(BinaryDataReader reader)
    {
        var length = reader.ReadByte();

        if (length == 0)
        {
            return string.Empty;
        }

        var encrypted = reader.ReadBytes(length);
        return DecryptString(encrypted);
    }

    private static string DecryptString(ReadOnlySpan<byte> data)
    {
        if (data.IsEmpty)
        {
            return string.Empty;
        }

        var decrypted = data.ToArray();

        for (var i = decrypted.Length - 1; i >= 0; i--)
        {
            decrypted[i] ^= CryptKey[i % CryptKey.Length];

            if (i > 0)
            {
                decrypted[i] ^= decrypted[i - 1];
            }
        }

        return Encoding.GetEncoding(850).GetString(decrypted);
    }
}