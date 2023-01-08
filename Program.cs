using System.Diagnostics;

if (args.Length is 0 || args.Contains("--help"))
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

// Execute benchmark
var workers = (0..workerCount).Select(_ => RunWorker());
var reports = Task.WhenAll(workers);
Console.WriteLine($"Workers 0-{workerCount - 1} started.");

var aggregated = (await reports).Aggregate((acc, r) => acc + r);
Console.WriteLine($"Workers 0-{workerCount - 1} stopped.");

// Print report(s)
// Console.WriteLine(string.Join('\n', reports));
Console.WriteLine(aggregated);

async Task<Report> RunWorker()
{
    var timestamp = Stopwatch.GetTimestamp();

    var successful = 0u;
    var failed = 0u;
    var kbTransferred = 0d;

    using var http = new HttpClient();

    while (!cts.IsCancellationRequested)
    {
        try
        {
            var response = await http.GetAsync(url, cts.Token);
            _ = response.IsSuccessStatusCode
                ? successful++
                : failed++;

            kbTransferred += (response.Content.Headers.ContentLength ?? 0D) / 1024D;
        }
        catch (Exception)
        {
            if (!cts.IsCancellationRequested)
            {
                failed++;
            }
        }
    }

    var seconds = Stopwatch.GetElapsedTime(timestamp).TotalSeconds;
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
