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
            throw new InvalidDataException("Unexpected end of data while reading UInt16.");
        }

        var value = BinaryPrimitives.ReadUInt16LittleEndian(_data.AsSpan(_position));
        _position += sizeof(ushort);
        return value;
    }

    public byte ReadByte()
    {
        if (_position >= _data.Length)
        {
            throw new InvalidDataException("Unexpected end of data while reading byte.");
        }

        return _data[_position++];
    }

    public uint ReadUInt32()
    {
        if (_position + sizeof(uint) > _data.Length)
        {
            throw new InvalidDataException("Unexpected end of data while reading UInt32.");
        }

        var value = BinaryPrimitives.ReadUInt32LittleEndian(_data.AsSpan(_position));
        _position += sizeof(uint);
        return value;
    }

    public ushort ReadBigEndianUInt16()
    {
        if (_position + sizeof(ushort) > _data.Length)
        {
            throw new InvalidDataException("Unexpected end of data while reading big-endian UInt16.");
        }

        var value = BinaryPrimitives.ReadUInt16BigEndian(_data.AsSpan(_position));
        _position += sizeof(ushort);
        return value;
    }

    public sbyte ReadSByte()
    {
        return unchecked((sbyte)ReadByte());
    }

    public short ReadInt16()
    {
        if (_position + sizeof(short) > _data.Length)
        {
            throw new InvalidDataException("Unexpected end of data while reading Int16.");
        }

        var value = BinaryPrimitives.ReadInt16LittleEndian(_data.AsSpan(_position));
        _position += sizeof(short);
        return value;
    }

    public int ReadInt32()
    {
        if (_position + sizeof(int) > _data.Length)
        {
            throw new InvalidDataException("Unexpected end of data while reading Int32.");
        }

        var value = BinaryPrimitives.ReadInt32LittleEndian(_data.AsSpan(_position));
        _position += sizeof(int);
        return value;
    }

    public bool ReadBoolean()
    {
        return ReadByte() != 0;
    }

    public string ReadPascalString(int fieldSize)
    {
        if (fieldSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(fieldSize));
        }

        var nameLength = ReadByte();
        var bytes = ReadBytes(fieldSize);
        var length = Math.Min(nameLength, fieldSize);
        return System.Text.Encoding.Latin1.GetString(bytes[..length]).TrimEnd();
    }

    public ReadOnlySpan<byte> ReadBytes(int count)
    {
        if (count < 0 || _position + count > _data.Length)
        {
            throw new InvalidDataException("Unexpected end of data while reading byte span.");
        }

        var value = _data.AsSpan(_position, count);
        _position += count;
        return value;
    }
}