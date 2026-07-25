namespace SignalViewerPlayground.Protocol;

public static class FoundSignalMessageSerializer
{
    /// <summary>
    /// This app only ever sends one kind of message, so 0x00 is simply its type code —
    /// not a stand-in for a broader type table (the wire format still requires *a* value here).
    /// </summary>
    public const byte MessageType = 0x00;

    public const int TotalSizeBytes = MessageHeader.SizeBytes + FoundSignalPayload.SizeBytes; // 30

    public static byte[] Serialize(FoundSignalPayload payload)
    {
        var buffer = new byte[TotalSizeBytes];

        MessageHeader.Pack((ushort)TotalSizeBytes, MessageType).WriteTo(buffer.AsSpan(0, MessageHeader.SizeBytes));
        payload.WriteTo(buffer.AsSpan(MessageHeader.SizeBytes));

        return buffer;
    }

    public static FoundSignalPayload Deserialize(ReadOnlySpan<byte> message)
    {
        var header = MessageHeader.ReadFrom(message[..MessageHeader.SizeBytes]);

        if (header.Length != TotalSizeBytes)
            throw new InvalidDataException($"Unexpected message length {header.Length}, expected {TotalSizeBytes}.");

        return FoundSignalPayload.ReadFrom(message[MessageHeader.SizeBytes..]);
    }
}
