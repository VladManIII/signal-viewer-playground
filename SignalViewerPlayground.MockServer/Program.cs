using SignalViewerPlayground.MockServer;

var options = MockServerOptions.Parse(args);
using var cts = new CancellationTokenSource();

Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };

Console.WriteLine($"[MockServer] port={options.Port} interval={options.SendInterval} seed={options.Seed?.ToString() ?? "random"}");

var generator = new MockSignalGenerator(options.Seed);
var server = new MockTcpServer(options.Port, options.SendInterval, generator);

await server.RunAsync(cts.Token);
