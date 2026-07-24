namespace SignalViewerPlayground.MockServer;

public sealed record MockServerOptions(int Port, TimeSpan SendInterval, int? Seed)
{
    public const int DefaultPort = 1488;
    public static readonly TimeSpan DefaultInterval = TimeSpan.FromMilliseconds(250);

    public static MockServerOptions Parse(string[] args)
    {
        int port = DefaultPort;
        TimeSpan interval = DefaultInterval;
        int? seed = null;

        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--port" when i + 1 < args.Length:
                    port = int.Parse(args[++i]);
                    break;
                case "--interval-ms" when i + 1 < args.Length:
                    interval = TimeSpan.FromMilliseconds(int.Parse(args[++i]));
                    break;
                case "--seed" when i + 1 < args.Length:
                    seed = int.Parse(args[++i]);
                    break;
            }
        }

        return new MockServerOptions(port, interval, seed);
    }
}
