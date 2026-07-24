using System.Buffers.Binary;

namespace SignalViewerPlayground.Protocol;

public readonly record struct MessageHeader(ushort Length, byte Type)
{
    public const int SizeBytes = 2;

    private const ushort LengthMask = 0x1FFF; // 13 bits
    private const int TypeShift = 13; 
    private const byte TypeMask = 0x07; // 3 bits

    public static MessageHeader Pack(ushort length, byte type)
    {
        if (length > LengthMask)
            throw new ArgumentOutOfRangeException(nameof(length), length, $"Length must fit in 13 bits (max {LengthMask}).");

        if (type > TypeMask)
            throw new ArgumentOutOfRangeException(nameof(type), type, $"Type must fit in 3 bits (max {TypeMask}).");

        return new MessageHeader(length, type);
    }

    public ushort ToUInt16() => (ushort)((Type << TypeShift) | (Length & LengthMask));

    public void WriteTo(Span<byte> destination) =>
        BinaryPrimitives.WriteUInt16LittleEndian(destination, ToUInt16());

    public static MessageHeader ReadFrom(ReadOnlySpan<byte> source)
    {
        ushort raw = BinaryPrimitives.ReadUInt16LittleEndian(source);

        return new MessageHeader((ushort)(raw & LengthMask), (byte)((raw >> TypeShift) & TypeMask));
    }
}
