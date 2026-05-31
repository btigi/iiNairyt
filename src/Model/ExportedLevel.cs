namespace ii.Nairyt.Model;

public record ExportedLevel(string Name, string Content)
{
    public ExportedImage? Tileset { get; init; }
}