using System.Buffers.Binary;

namespace SignalViewerPlayground.Protocol;

/// <summary>
/// TimestampRaw unit is unspecified by the source spec; the mock generator treats it
/// as Unix epoch milliseconds. Frequency/Bandwidth are in Hz.
/// </summary>
public readonly record struct FoundSignalPayload(
    ulong TimestampRaw,
    ulong FrequencyHz,
    uint BandwidthHz,
    double SnrDb)
{
    public const int SizeBytes = 8 + 8 + 4 + 8; // 28

    public void WriteTo(Span<byte> destination)
    {
        BinaryPrimitives.WriteUInt64LittleEndian(destination[0..8], TimestampRaw);
        BinaryPrimitives.WriteUInt64LittleEndian(destination[8..16], FrequencyHz);
        BinaryPrimitives.WriteUInt32LittleEndian(destination[16..20], BandwidthHz);
        BinaryPrimitives.WriteDoubleLittleEndian(destination[20..28], SnrDb);
    }

    public static FoundSignalPayload ReadFrom(ReadOnlySpan<byte> source) => new(
        BinaryPrimitives.ReadUInt64LittleEndian(source[0..8]),
        BinaryPrimitives.ReadUInt64LittleEndian(source[8..16]),
        BinaryPrimitives.ReadUInt32LittleEndian(source[16..20]),
        BinaryPrimitives.ReadDoubleLittleEndian(source[20..28]));
}
