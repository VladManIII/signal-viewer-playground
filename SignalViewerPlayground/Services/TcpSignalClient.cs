using System.IO;
using System.Net.Sockets;
using SignalViewerPlayground.Protocol;

namespace SignalViewerPlayground.Services;

/// <summary>
/// Connects to the mock/real signal TCP server and prints each decoded
/// FoundSignalPayload to the console as it arrives.
/// </summary>
public sealed class TcpSignalClient
{
    public async Task ConnectAndStreamAsync(string host, int port, CancellationToken cancellationToken)
    {
        using var client = new TcpClient();

        await client.ConnectAsync(host, port, cancellationToken);
        Console.WriteLine($"[TcpSignalClient] Connected to {host}:{port}");

        using var stream = client.GetStream();
        var headerBuffer = new byte[MessageHeader.SizeBytes];

        while (!cancellationToken.IsCancellationRequested)
        {
            await ReadExactAsync(stream, headerBuffer, cancellationToken);
            var header = MessageHeader.ReadFrom(headerBuffer);

            var message = new byte[header.Length];
            headerBuffer.CopyTo(message, 0);
            await ReadExactAsync(stream, message.AsMemory(MessageHeader.SizeBytes), cancellationToken);

            var payload = FoundSignalMessageSerializer.Deserialize(message);
            Console.WriteLine(
                $"[TcpSignalClient] ts={payload.TimestampRaw} " +
                $"freq={payload.FrequencyHz / 1_000_000.0:F3}MHz " +
                $"bw={payload.BandwidthHz / 1000.0:F1}kHz " +
                $"snr={payload.SnrDb:F1}dB");
        }
    }

    private static async Task ReadExactAsync(NetworkStream stream, Memory<byte> buffer, CancellationToken cancellationToken)
    {
        int offset = 0;
        while (offset < buffer.Length)
        {
            int read = await stream.ReadAsync(buffer[offset..], cancellationToken);
            if (read == 0)
                throw new IOException("Connection closed by remote host.");
            offset += read;
        }
    }
}
