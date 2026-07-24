namespace SignalViewerPlayground.Protocol;

public static class FoundSignalMessageSerializer
{
    /// <summary>
    /// The spec does not define a concrete Type code for a "found signal" message.
    /// 0x00 is a placeholder pending a real NetSDR-style type table.
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
