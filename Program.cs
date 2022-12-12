using System.Diagnostics;

if (args.Contains("--help") || args.Length is 0)
{
    Console.WriteLine("""
        args:
            url, target address
            -p {num}, number of parallel workers
            -t {secs}, execution time in seconds

        usage: http://address.example -p 64 -t 120
        """);
    return;
}

var url = new Uri(args.First(arg => arg.StartsWith("http")));
var executionTime = GetArg<uint>("-t");
var workerCount = GetArg<int>("-p");

using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(executionTime));

var workers = (0..workerCount)
    .AsEnumerable()
    .Select(RunWorker);

// Execute benchmark
var reports = await Task.WhenAll(workers);
var aggregated = reports.Aggregate(default(Report), (seed, r) => seed + r);

// Print report(s)
// Console.WriteLine(string.Join('\n', reports));
Console.WriteLine(aggregated);

async Task<Report> RunWorker(int i)
{
    Console.WriteLine($"Starting worker {i}!");
    var timestamp = Stopwatch.GetTimestamp();

    var successful = 0u;
    var failed = 0u;
    var kbTransferred = 0d;

    using var http = new HttpClient();

    var ct = cts.Token;
    while (!ct.IsCancellationRequested)
    {
        var response = await http.GetAsync(url, CancellationToken.None);
        if (response.IsSuccessStatusCode)
        {
            successful++;
            kbTransferred += (response.Content.Headers.ContentLength ?? 0D) / 1024D;
        }
        else
        {
            failed++;
        }
    }

    var seconds = Stopwatch.GetElapsedTime(timestamp).TotalSeconds;
    Console.WriteLine($"Worker {i} stopped!");

    return new(
        Successful: successful,
        Failed: failed,
        MegabytesPerSecond: (float)(kbTransferred / 1024 / seconds),
        RPS: (float)((successful + failed) / seconds));
}

T GetArg<T>(string key) where T : IParsable<T>
{
    var idx = args.AsSpan().IndexOf(key);

    return idx > -1
        ? T.Parse(args[idx + 1], null)
        : throw new ArgumentException($"Argument {key} was not supplied. Exiting.");
}

readonly record struct Report(
    ulong Successful,
    ulong Failed,
    float MegabytesPerSecond,
    float RPS)
{
    public static Report operator +(Report left, Report right) => new(
        left.Successful + right.Successful,
        left.Failed + right.Failed,
        left.MegabytesPerSecond + right.MegabytesPerSecond,
        left.RPS + right.RPS);
}
