### Run
- Download an executable for your OS from https://github.com/neon-sunset/http-bench/releases
- Run in console e.g. `http-bench http://address.example -c 64 -t 120`

~~AOT version has faster startup and smaller binary, JIT has higher sustained throughput.~~

There seems to be little profit in using self-contained JIT build in .NET 8 for CLI utils.
Therefore, `PublishAot` is set by default in .csproj but you can always remove it and build with `PublishSingleFile` and `PublishTrimmed` instead.

### Args
```sh
url, target address
-c {num}, number of concurrent workers e.g. simulated user count
-t {secs}, execution time in seconds

usage: http://address.example -c 64 -t 120
```

### Build
- Download and install .NET 8 preview SDK from https://github.com/dotnet/installer#table
- `make` or `dotnet publish -c release -o publish --use-current-runtime`
