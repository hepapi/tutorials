# .NET Example

This sample is a minimal ASP.NET Core application that returns `Hello, World!`.

## Run Without Docker

### Required Tools
- .NET 6 SDK

### Start Locally
```bash
cd Docker/example-projects/dotnet
dotnet restore
dotnet run
```

Open http://localhost:8080

## Notes
- This example is intentionally small and works well for explaining image build steps and multi-stage Dockerfiles.
- If `dotnet` is not installed, install the .NET 6 SDK first.

## Useful Commands
```bash
dotnet restore
dotnet run
dotnet publish -c Release
```
