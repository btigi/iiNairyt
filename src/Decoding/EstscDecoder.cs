using ii.Nairyt.Imaging;

namespace ii.Nairyt.Decoding;

internal class EstscDecoder
{
    private readonly BinaryDataReader _reader;

    public EstscDecoder(byte[] data)
    {
        _reader = new BinaryDataReader(data);
    }

    public IReadOnlyList<IndexedImage?> LoadImages()
    {
        var imageCount = _reader.ReadUInt16();
        var images = new List<IndexedImage?>(imageCount);

        for (var i = 0; i < imageCount; i++)
        {
            var populated = _reader.ReadByte();

            if (populated == 0)
            {
                images.Add(null);
                continue;
            }

            var width = _reader.ReadUInt16();
            var height = _reader.ReadUInt16();
            var compressedSize = _reader.ReadUInt16();
            var compressed = _reader.ReadBytes(compressedSize);
            images.Add(DecodeRle(width, height, compressed));
        }

        return images;
    }

    public void Seek(int position)
    {
        _reader.Seek(position);
    }

    private static IndexedImage DecodeRle(int width, int height, ReadOnlySpan<byte> compressed)
    {
        var image = new IndexedImage(width, height);
        var offset = 0;
        var x = 0;
        var y = 0;

        while (offset < compressed.Length)
        {
            var command = compressed[offset++];

            switch (command)
            {
                case TyrianConstants.RleSkipPixels:
                    x += compressed[offset++];
                    break;

                case TyrianConstants.RleNextRow:
                    y++;
                    x = 0;
                    break;

                case TyrianConstants.RleTransparentPixel:
                    image.SetPixel(x, y, 0);
                    x++;
                    break;

                default:
                    image.SetPixel(x, y, command);
                    x++;
                    break;
            }

            if (x >= width)
            {
                y++;
                x = 0;
            }
        }

        return image;
    }
}