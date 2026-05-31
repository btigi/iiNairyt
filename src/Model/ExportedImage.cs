using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace ii.Nairyt.Model;

public record ExportedImage(string Name, Image<Rgba32> Image);