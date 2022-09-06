### Install
- Download an executable for your OS from https://github.com/neon-sunset/http-bench/releases
- Run in console e.g. `http-bench https://example.org -p 64 -t 120`

AOT version has faster startup and smaller binary, JIT has higher sustained throughput.
Keep in mind that NativeAOT is currently unsupported on macOS (subject to change).

### Args
```sh
url, target address
-p {num}, number of parallel workers
-t {secs}, execution time in seconds

usage: http://address.example -p 64 -t 120
```

### Build
Download SDK from https://dotnet.microsoft.com/en-us/download/dotnet/7.0

JIT version
```sh
dotnet publish -c release -o publish -p:PublishSingleFile=true -p:PublishTrimmed=true --use-current-runtime --self-contained
```

AOT version
```sh
dotnet publish -c release -o publish -p:PublishAot=true --use-current-runtime
```
Alternatively, you can simply uncomment `<PublishAot>true</PublishAot>` in .csproj file.
