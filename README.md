## Build
Download https://dotnet.microsoft.com/en-us/download/dotnet/7.0

JIT version
```sh
dotnet publish -c release -o publish -p:PublishSingleFile=true -p:PublishTrimmed=true --use-current-runtime --self-contained
```

AOT version
```sh
dotnet publish -c release -o publish -p:PublishAot=true --use-current-runtime
```
Alternatively, you can simply uncomment `<PublishAot>true</PublishAot>` in .csproj file.

## Run
```sh
http-bench https://example.org -p 64 -t 120
```