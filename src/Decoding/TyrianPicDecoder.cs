using ii.Nairyt.Imaging;

namespace ii.Nairyt.Decoding;

internal static class TyrianPicDecoder
{
    public static IReadOnlyList<IndexedImage> DecodeAll(byte[] data)
    {
        var reader = new BinaryDataReader(data);
        _ = reader.ReadUInt16();

        var offsets = new int[TyrianConstants.PicImageCount + 1];
        for (var i = 0; i < TyrianConstants.PicImageCount; i++)
        {
            offsets[i] = reader.ReadInt32();
        }

        offsets[TyrianConstants.PicImageCount] = data.Length;

        ValidateOffsets(offsets, data.Length);

        var images = new List<IndexedImage>(TyrianConstants.PicImageCount);

        for (var imageIndex = 0; imageIndex < TyrianConstants.PicImageCount; imageIndex++)
        {
            var start = offsets[imageIndex];
            var size = offsets[imageIndex + 1] - start;

            if (size <= 0)
            {
                throw new InvalidDataException($"PIC image {imageIndex} has invalid size {size}.");
            }

            var compressed = data.AsSpan(start, size);
            var image = new IndexedImage(TyrianConstants.PicWidth, TyrianConstants.PicHeight);
            DecodeRle(compressed, image);
            images.Add(image);
        }

        return images;
    }

    private static void ValidateOffsets(int[] offsets, int fileLength)
    {
        if (offsets[0] < 0)
        {
            throw new InvalidDataException("PIC offset table starts before file beginning.");
        }

        for (var i = 0; i < offsets.Length; i++)
        {
            if (offsets[i] > fileLength)
            {
                throw new InvalidDataException($"PIC offset {i} ({offsets[i]}) is beyond file length {fileLength}.");
            }

            if (i > 0 && offsets[i] < offsets[i - 1])
            {
                throw new InvalidDataException($"PIC offsets are not monotonic at index {i}.");
            }
        }
    }

    private static void DecodeRle(ReadOnlySpan<byte> compressed, IndexedImage image)
    {
        var offset = 0;
        var pixelIndex = 0;
        var pixelCount = TyrianConstants.PicWidth * TyrianConstants.PicHeight;

        while (offset < compressed.Length && pixelIndex < pixelCount)
        {
            var command = compressed[offset++];

            if ((command & TyrianConstants.PicRleRunMarker) == TyrianConstants.PicRleRunMarker)
            {
                if (offset >= compressed.Length)
                {
                    throw new InvalidDataException("PIC RLE run is missing its color byte.");
                }

                var runLength = command & TyrianConstants.PicRleRunLengthMask;
                var color = compressed[offset++];
                var runEnd = Math.Min(pixelIndex + runLength, pixelCount);

                for (var i = pixelIndex; i < runEnd; i++)
                {
                    image.Pixels[i] = color;
                }

                pixelIndex += runLength;
            }
            else
            {
                image.Pixels[pixelIndex] = command;
                pixelIndex++;
            }
        }

        if (pixelIndex < pixelCount)
        {
            throw new InvalidDataException($"PIC RLE stream ended early at {pixelIndex}/{pixelCount} pixels.");
        }
    }
}