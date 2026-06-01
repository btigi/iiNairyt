using System.Buffers.Binary;
using System.Text;

namespace ii.Nairyt.Decoding;

internal class BinaryDataWriter
{
    private readonly byte[] _buffer;
    private int _position;

    public BinaryDataWriter(int capacity)
    {
        _buffer = new byte[capacity];
    }

    public int Position => _position;

    public void Seek(int position)
    {
        if (position < 0 || position > _buffer.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(position));
        }

        _position = position;
    }

    public byte[] ToArray() => _buffer;

    public void WriteUInt16(ushort value)
    {
        BinaryPrimitives.WriteUInt16LittleEndian(_buffer.AsSpan(_position), value);
        _position += sizeof(ushort);
    }

    public void WriteByte(byte value)
    {
        _buffer[_position++] = value;
    }

    public void WriteInt32(int value)
    {
        BinaryPrimitives.WriteInt32LittleEndian(_buffer.AsSpan(_position), value);
        _position += sizeof(int);
    }

    public void WriteSByte(sbyte value)
    {
        WriteByte(unchecked((byte)value));
    }

    public void WriteInt16(short value)
    {
        BinaryPrimitives.WriteInt16LittleEndian(_buffer.AsSpan(_position), value);
        _position += sizeof(short);
    }

    public void WriteBoolean(bool value)
    {
        WriteByte(value ? (byte)1 : (byte)0);
    }

    public void WritePascalString(string value, int fieldSize)
    {
        if (fieldSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(fieldSize));
        }

        var bytes = Encoding.Latin1.GetBytes(value);
        var length = Math.Min(bytes.Length, fieldSize);
        WriteByte((byte)length);

        for (var i = 0; i < length; i++)
        {
            WriteByte(bytes[i]);
        }

        for (var i = length; i < fieldSize; i++)
        {
            WriteByte(0);
        }
    }

    public void WriteZeroBytes(int count)
    {
        if (count <= 0)
        {
            return;
        }

        Array.Clear(_buffer, _position, count);
        _position += count;
    }
}