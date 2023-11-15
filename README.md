### Args
```sh
url, target address
-c {num}, number of concurrent workers e.g. simulated user count
-t {secs}, execution time in seconds

usage: http://address.example -c 64 -t 120
```

### Build
- Download and install .NET 8 SDK at https://dot.net/download
- `make` or `dotnet publish -o publish`
