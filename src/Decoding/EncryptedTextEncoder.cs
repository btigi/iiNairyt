using System.Text;

namespace ii.Nairyt.Decoding;

internal static class EncryptedTextEncoder
{
    private static readonly byte[] CryptKey = [204, 129, 63, 255, 71, 19, 25, 62, 1, 99];

    static EncryptedTextEncoder()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    public static byte[] WriteAllText(string text)
    {
        using var output = new MemoryStream();
        using var reader = new StringReader(text);

        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            WriteEncryptedPascalString(output, line);
        }

        return output.ToArray();
    }

    private static void WriteEncryptedPascalString(Stream output, string value)
    {
        var plain = Encoding.GetEncoding(850).GetBytes(value);

        if (plain.Length > byte.MaxValue)
        {
            throw new InvalidDataException($"Encrypted text line exceeds the maximum length of {byte.MaxValue} bytes.");
        }

        var encrypted = EncryptString(plain);
        output.WriteByte((byte)encrypted.Length);
        output.Write(encrypted);
    }

    private static byte[] EncryptString(ReadOnlySpan<byte> plain)
    {
        if (plain.IsEmpty)
        {
            return [];
        }

        var encrypted = new byte[plain.Length];
        encrypted[0] = (byte)(plain[0] ^ CryptKey[0]);

        for (var i = 1; i < plain.Length; i++)
        {
            encrypted[i] = (byte)(plain[i] ^ CryptKey[i % CryptKey.Length] ^ encrypted[i - 1]);
        }

        return encrypted;
    }
}