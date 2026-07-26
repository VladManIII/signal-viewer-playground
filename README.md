# signal-viewer-playground

A .NET 8 / WPF desktop app that connects to a TCP server streaming decoded
radio signal detections, aggregates them into records by frequency/bandwidth
band, and displays them in a filterable, sortable table.

Each record shows the timestamp of the first signal in the run, the band's
frequency (re-based to the median frequency of all matched signals once the
run ends) and bandwidth, SNR, and how many signals matched. The table can be
filtered by a set of preset frequency bands (matching the NetSDR RF Filter
Selection bands) or a custom min/max range. To keep memory usage bounded,
only the most recent 1000 records are kept — older records are dropped once
that limit is reached.

No real signal source is required — a mock TCP server ships alongside the
app for local development and demoing.

## Requirements

- .NET 8 SDK, with the Windows desktop workload (the main app is
  `net8.0-windows` WPF and only runs on Windows)

## Running the app

The app is just a TCP client — it needs something to connect to.

The preferred way to run it is via the **"Start with TCP mock"** launch
option in Visual Studio (the solution's default multi-project startup
profile) — it starts the mock server and the WPF app together in one go.

Alternatively, start each project manually, from the command line, in its
own terminal — the mock server **first**, then the main app:

```
# Terminal 1 - mock signal server (defaults to 127.0.0.1:1488)
dotnet run --project SignalViewerPlayground.MockServer/SignalViewerPlayground.MockServer.csproj

# Terminal 2 - the WPF app
dotnet run --project SignalViewerPlayground/SignalViewerPlayground.csproj
```

The app connects to `127.0.0.1:1488` on startup and will show a connection
error in the status bar if the mock server isn't running yet.

The mock server accepts optional arguments:

```
dotnet run --project SignalViewerPlayground.MockServer/SignalViewerPlayground.MockServer.csproj -- --port 1488 --interval-ms 250 --seed 42
```

- `--port` — TCP port to listen on (default `1488`)
- `--interval-ms` — delay between generated signals (default `250`)
- `--seed` — fixes the random generator for reproducible runs (default: random)

## Running the tests

```
dotnet test SignalViewerPlayground.Tests/SignalViewerPlayground.Tests.csproj
dotnet test SignalViewerPlayground.Protocol.Tests/SignalViewerPlayground.Protocol.Tests.csproj
```
