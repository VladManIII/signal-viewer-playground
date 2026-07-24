namespace SignalViewerPlayground.Protocol.Tests;

[TestFixture]
public class MessageHeaderTests
{
    [Test]
    public void Pack_And_WriteTo_MatchesSpecBitLayout_ForTypicalMessage()
    {
        var header = MessageHeader.Pack(length: 30, type: 0);
        var buffer = new byte[2];
        header.WriteTo(buffer);

        Assert.That(buffer, Is.EqualTo(new byte[] { 0x1E, 0x00 }));
    }

    [Test]
    public void Pack_And_WriteTo_MatchesSpecBitLayout_ForMaxValues()
    {
        var header = MessageHeader.Pack(length: 8191, type: 7);
        var buffer = new byte[2];
        header.WriteTo(buffer);

        Assert.That(buffer, Is.EqualTo(new byte[] { 0xFF, 0xFF }));
    }

    [TestCase((ushort)0, (byte)0)]
    [TestCase((ushort)30, (byte)0)]
    [TestCase((ushort)8191, (byte)7)]
    [TestCase((ushort)255, (byte)3)]
    [TestCase((ushort)4096, (byte)4)]
    public void WriteTo_ReadFrom_RoundTrips(ushort length, byte type)
    {
        var header = MessageHeader.Pack(length, type);
        var buffer = new byte[2];
        header.WriteTo(buffer);

        var roundTripped = MessageHeader.ReadFrom(buffer);

        Assert.That(roundTripped.Length, Is.EqualTo(length));
        Assert.That(roundTripped.Type, Is.EqualTo(type));
    }

    [Test]
    public void Pack_Throws_WhenLengthExceeds13Bits()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => MessageHeader.Pack(8192, 0));
    }

    [Test]
    public void Pack_Throws_WhenTypeExceeds3Bits()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => MessageHeader.Pack(30, 8));
    }
}
