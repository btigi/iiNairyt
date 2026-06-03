using ii.Nairyt.Decoding;
using ii.Nairyt.Model;

namespace ii.Nairyt;

public static class DatProcessor
{
    public static ExportedText Read(string datPath)
    {
        ArgumentException.ThrowIfNullOrEmpty(datPath);

        if (!File.Exists(datPath))
        {
            throw new FileNotFoundException($"DAT file not found: {datPath}", datPath);
        }

        var data = File.ReadAllBytes(datPath);
        var baseName = Path.GetFileNameWithoutExtension(datPath);

        return new ExportedText(baseName, EncryptedTextDecoder.ReadAllText(data));
    }

    public static void Write(string text, string datPath)
    {
        ArgumentNullException.ThrowIfNull(text);
        ArgumentException.ThrowIfNullOrEmpty(datPath);

        var data = EncryptedTextEncoder.WriteAllText(text);
        File.WriteAllBytes(datPath, data);
    }
}