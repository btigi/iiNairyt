using ii.Nairyt.Decoding;
using ii.Nairyt.Model;

namespace ii.Nairyt;

public static class HdtProcessor
{
    public static TyrianHdtData Read(string hdtPath)
    {
        ArgumentException.ThrowIfNullOrEmpty(hdtPath);

        if (!File.Exists(hdtPath))
        {
            throw new FileNotFoundException($"HDT file not found: {hdtPath}", hdtPath);
        }

        var data = File.ReadAllBytes(hdtPath);
        return new TyrianHdtDecoder(data).Decode();
    }
}