﻿using System.Diagnostics;
using System.Runtime.InteropServices;
using static System.Console;

if (args.Length is 0 || args.Contains("--help"))
{
    WriteLine("""
        args:
            url, target address
            -c {num}, number of concurrent workers e.g. simulated user count
            -t {secs}, execution time in seconds

        usage: http://address.example -c 64 -t 120
        """);
    return;
}

var url = new Uri(args.First(arg => arg.StartsWith("http")));
var time = GetArg<uint>("-t");
var concurrency = GetArg<int>("-c");

using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(time));
using var sigint = PosixSignalRegistration.Create(PosixSignal.SIGINT, ctx =>
{
    // Cancel default handling and let '$Main' exit gracefully
    WriteLine("Stopping...");
    ctx.Cancel = true;
    cts.Cancel();
});

// Start workers
var workers = Enumerable
    .Repeat(RunWorker, concurrency)
    .Select(w => w.Invoke());
var reports = Task.WhenAll(workers);
WriteLine($"Workers 0-{concurrency - 1} started.");

// Wait for all workers to finish
var aggregated = (await reports).Aggregate((acc, r) => acc + r);
WriteLine($"Workers 0-{concurrency - 1} stopped.");

// Print report(s)
// WriteLine(string.Join('\n', reports.Result));
WriteLine(aggregated);

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
            if (response.IsSuccessStatusCode) successful++;
            else failed++;

            kbTransferred += (response.Content.Headers.ContentLength ?? 0D) / 1024D;
        }
        catch (Exception)
        {
            if (!cts.IsCancellationRequested) failed++;
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
