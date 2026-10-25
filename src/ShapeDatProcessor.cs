using ii.Nairyt.Decoding;
using ii.Nairyt.Imaging;
using ii.Nairyt.Model;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace ii.Nairyt;

public static class ShapeDatProcessor
{
    public static ShapeDatData Read(string shapeDatPath)
    {
        ArgumentException.ThrowIfNullOrEmpty(shapeDatPath);

        if (!File.Exists(shapeDatPath))
        {
            throw new FileNotFoundException($"Shape DAT file not found: {shapeDatPath}", shapeDatPath);
        }

        var decoded = ShapeDatDecoder.Decode(File.ReadAllBytes(shapeDatPath));
        return new ShapeDatData(decoded.Sheet.Width, decoded.Sheet.Height, decoded.Sheet.Pixels, decoded.Trailer);
    }

    public static void Write(ShapeDatData data, string shapeDatPath)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentException.ThrowIfNullOrEmpty(shapeDatPath);

        File.WriteAllBytes(shapeDatPath, ShapeDatEncoder.Encode(data));
    }

    public static ExportedImage Export(ShapeDatData data, string palettePath, string name)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentException.ThrowIfNullOrEmpty(palettePath);
        ArgumentException.ThrowIfNullOrEmpty(name);

        if (!File.Exists(palettePath))
        {
            throw new FileNotFoundException($"Palette file not found: {palettePath}", palettePath);
        }

        return new ExportedImage(name, ToRgba(data, TyrianPalette.Load(palettePath)));
    }

    internal static Image<Rgba32> ToRgba(ShapeDatData data, Rgba32[] palette)
    {
        ArgumentNullException.ThrowIfNull(palette);
        ShapeDatEncoder.EnsureSheet(data);

        var sheet = new IndexedImage(data.Width, data.Height);
        data.Pixels.CopyTo(sheet.Pixels.AsSpan());
        return sheet.ToRgba32(palette);
    }
}