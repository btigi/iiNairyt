using System.Buffers.Binary;

namespace ii.Nairyt.Decoding;

internal class BinaryDataReader
{
    private readonly byte[] _data;
    private int _position;

    public BinaryDataReader(byte[] data)
    {
        _data = data;
    }

    public int Position => _position;

    public void Seek(int position)
    {
        if (position < 0 || position > _data.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(position));
        }

        _position = position;
    }

    public ushort ReadUInt16()
    {
        if (_position + sizeof(ushort) > _data.Length)
        {
            throw new InvalidDataException("Unexpected end of SHP data while reading UInt16.");
        }

        var value = BinaryPrimitives.ReadUInt16LittleEndian(_data.AsSpan(_position));
        _position += sizeof(ushort);
        return value;
    }

    public byte ReadByte()
    {
        if (_position >= _data.Length)
        {
            throw new InvalidDataException("Unexpected end of SHP data while reading byte.");
        }

        return _data[_position++];
    }

    public ReadOnlySpan<byte> ReadBytes(int count)
    {
        if (count < 0 || _position + count > _data.Length)
        {
            throw new InvalidDataException("Unexpected end of SHP data while reading byte span.");
        }

        var value = _data.AsSpan(_position, count);
        _position += count;
        return value;
    }
}