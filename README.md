### Install
- Download an executable for your OS from https://github.com/neon-sunset/http-bench/releases
- Run in console e.g. `http-bench https://example.org -p 64 -t 120`

~~AOT version has faster startup and smaller binary, JIT has higher sustained throughput.~~

There seems to be little profit in using self-contained JIT build in .NET 8 for CLI utils.
Threfore, `PublishAot` is set by default in .csproj but you can always remove it and build with `PublishSingleFile` and `PublishTrimmed` instead.

### Args
```sh
url, target address
-p {num}, number of parallel workers
-t {secs}, execution time in seconds

usage: http://address.example -p 64 -t 120
```

### Build
Download 8.0 SDK from https://github.com/dotnet/installer#table

```sh
dotnet publish -c release -o publish --use-current-runtime
```
