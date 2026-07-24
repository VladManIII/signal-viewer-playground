using System.Buffers.Binary;

namespace SignalViewerPlayground.Protocol.Tests;

[TestFixture]
public class FoundSignalMessageSerializerTests
{
    [Test]
    public void Serialize_ProducesExactly30Bytes()
    {
        var payload = new FoundSignalPayload(1, 2, 3, 4.0);

        var message = FoundSignalMessageSerializer.Serialize(payload);

        Assert.That(message.Length, Is.EqualTo(30));
    }

    [TestCaseSource(nameof(RoundTripPayloads))]
    public void Serialize_Deserialize_RoundTrip_PreservesAllFields(FoundSignalPayload payload)
    {
        var message = FoundSignalMessageSerializer.Serialize(payload);

        var roundTripped = FoundSignalMessageSerializer.Deserialize(message);

        Assert.That(roundTripped, Is.EqualTo(payload));
    }

    private static IEnumerable<FoundSignalPayload> RoundTripPayloads()
    {
        yield return new FoundSignalPayload(0, 0, 0, 0.0);
        yield return new FoundSignalPayload(ulong.MinValue, ulong.MinValue, uint.MinValue, double.Epsilon);
        yield return new FoundSignalPayload(ulong.MaxValue, ulong.MaxValue, uint.MaxValue, -123.456);
        yield return new FoundSignalPayload(1_700_000_000_000, 100_000_000, 20_000, 12.5);
    }

    [Test]
    public void Serialize_WritesFieldsInDocumentedByteOrder()
    {
        var payload = new FoundSignalPayload(
            TimestampRaw: 0x0102030405060708,
            FrequencyHz: 0x1112131415161718,
            BandwidthHz: 0x21222324,
            SnrDb: 3.14159);

        var message = FoundSignalMessageSerializer.Serialize(payload);

        Assert.That(BinaryPrimitives.ReadUInt64LittleEndian(message.AsSpan(2, 8)), Is.EqualTo(payload.TimestampRaw));
        Assert.That(BinaryPrimitives.ReadUInt64LittleEndian(message.AsSpan(10, 8)), Is.EqualTo(payload.FrequencyHz));
        Assert.That(BinaryPrimitives.ReadUInt32LittleEndian(message.AsSpan(18, 4)), Is.EqualTo(payload.BandwidthHz));
        Assert.That(BinaryPrimitives.ReadDoubleLittleEndian(message.AsSpan(22, 8)), Is.EqualTo(payload.SnrDb));
    }

    [Test]
    public void Deserialize_Throws_WhenHeaderLengthDoesNotMatchExpectedSize()
    {
        var payload = new FoundSignalPayload(1, 2, 3, 4.0);
        var message = FoundSignalMessageSerializer.Serialize(payload);
        MessageHeader.Pack(29, FoundSignalMessageSerializer.MessageType).WriteTo(message.AsSpan(0, 2));

        Assert.Throws<InvalidDataException>(() => FoundSignalMessageSerializer.Deserialize(message));
    }
}
