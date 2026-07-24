using System.Net;
using System.Net.Sockets;
using SignalViewerPlayground.Protocol;

namespace SignalViewerPlayground.MockServer;

public sealed class MockTcpServer(int port, TimeSpan sendInterval, MockSignalGenerator generator)
{
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var listener = new TcpListener(IPAddress.Any, port);
        listener.Start();
        Console.WriteLine($"[MockServer] Listening on port {port}. Press Ctrl+C to stop.");

        var clientTasks = new List<Task>();
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var client = await listener.AcceptTcpClientAsync(cancellationToken);
                Console.WriteLine($"[MockServer] Client connected: {client.Client.RemoteEndPoint}");
                clientTasks.Add(HandleClientAsync(client, cancellationToken));
            }
        }
        catch (OperationCanceledException)
        {
            // normal shutdown
        }
        finally
        {
            listener.Stop();
            await Task.WhenAll(clientTasks.Select(SwallowErrors));
            Console.WriteLine("[MockServer] Shutdown complete.");
        }
    }

    private async Task HandleClientAsync(TcpClient client, CancellationToken cancellationToken)
    {
        using (client)
        using (var stream = client.GetStream())
        {
            try
            {
                using var timer = new PeriodicTimer(sendInterval);
                while (await timer.WaitForNextTickAsync(cancellationToken))
                {
                    var payload = generator.NextSignal();
                    await stream.WriteAsync(FoundSignalMessageSerializer.Serialize(payload), cancellationToken);
                    Console.WriteLine(
                        $"[MockServer] -> {client.Client.RemoteEndPoint}: " +
                        $"f={payload.FrequencyHz / 1_000_000.0:F3}MHz bw={payload.BandwidthHz / 1000.0:F1}kHz snr={payload.SnrDb:F1}dB");
                }
            }
            catch (Exception ex) when (ex is IOException or SocketException or OperationCanceledException)
            {
                Console.WriteLine($"[MockServer] Client disconnected: {client.Client.RemoteEndPoint} ({ex.GetType().Name})");
            }
        }
    }

    private static async Task SwallowErrors(Task task)
    {
        try
        {
            await task;
        }
        catch
        {
            // already logged at the point of failure; shutdown must not throw
        }
    }
}
