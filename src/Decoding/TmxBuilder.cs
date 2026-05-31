using System.Text;
using System.Xml;

namespace ii.Nairyt.Decoding;

internal static class TmxBuilder
{
    public static string Build(IReadOnlyList<IReadOnlyList<int>> layers, string tilesetName, string tilesetImageFileName, int tilesetImageWidth, int tilesetImageHeight)
    {
        var settings = new XmlWriterSettings
        {
            Indent = true,
            OmitXmlDeclaration = false,
            Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
        };

        using var stream = new MemoryStream();
        using (var writer = XmlWriter.Create(stream, settings))
        {
            writer.WriteStartDocument();
            writer.WriteStartElement("map");
            writer.WriteAttributeString("version", "1.10");
            writer.WriteAttributeString("tiledversion", "1.10.1");
            writer.WriteAttributeString("orientation", "orthogonal");
            writer.WriteAttributeString("renderorder", "right-down");
            writer.WriteAttributeString("width", TyrianConstants.LevelMapWidth.ToString());
            writer.WriteAttributeString("height", TyrianConstants.LevelMapHeight.ToString());
            writer.WriteAttributeString("tilewidth", TyrianConstants.LevelTileWidth.ToString());
            writer.WriteAttributeString("tileheight", TyrianConstants.LevelTileHeight.ToString());
            writer.WriteAttributeString("infinite", "0");

            WriteTileset(writer, tilesetName, tilesetImageFileName, tilesetImageWidth, tilesetImageHeight);
            WriteLayer(writer, 1, "Layer 1", layers[0], null, null);
            WriteLayer(writer, 2, "Layer 2", layers[1], null, "2");
            WriteLayer(writer, 3, "Layer 3", layers[2], "2", "2");

            writer.WriteEndElement();
            writer.WriteEndDocument();
        }

        return Encoding.UTF8.GetString(stream.ToArray());
    }

    private static void WriteTileset(XmlWriter writer, string tilesetName, string tilesetImageFileName, int tilesetImageWidth, int tilesetImageHeight)
    {
        writer.WriteStartElement("tileset");
        writer.WriteAttributeString("firstgid", "1");
        writer.WriteAttributeString("name", tilesetName);
        writer.WriteAttributeString("tilewidth", TyrianConstants.LevelTileWidth.ToString());
        writer.WriteAttributeString("tileheight", TyrianConstants.LevelTileHeight.ToString());
        writer.WriteAttributeString("tilecount", TyrianConstants.ShapeTileCount.ToString());
        writer.WriteAttributeString("columns", TyrianConstants.ShapeColumns.ToString());

        writer.WriteStartElement("image");
        writer.WriteAttributeString("source", tilesetImageFileName);
        writer.WriteAttributeString("width", tilesetImageWidth.ToString());
        writer.WriteAttributeString("height", tilesetImageHeight.ToString());
        writer.WriteEndElement();

        writer.WriteEndElement();
    }

    private static void WriteLayer(XmlWriter writer, int layerId, string layerName, IReadOnlyList<int> tileData, string? parallaxX, string? parallaxY)
    {
        writer.WriteStartElement("layer");
        writer.WriteAttributeString("id", layerId.ToString());
        writer.WriteAttributeString("name", layerName);
        writer.WriteAttributeString("width", TyrianConstants.LevelMapWidth.ToString());
        writer.WriteAttributeString("height", TyrianConstants.LevelMapHeight.ToString());

        if (parallaxX is not null)
        {
            writer.WriteAttributeString("parallaxx", parallaxX);
        }

        if (parallaxY is not null)
        {
            writer.WriteAttributeString("parallaxy", parallaxY);
        }

        writer.WriteStartElement("data");
        writer.WriteAttributeString("encoding", "csv");
        writer.WriteString(string.Join(',', tileData));
        writer.WriteEndElement();

        writer.WriteEndElement();
    }
}