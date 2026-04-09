# .NET Example

This sample is a small ASP.NET Core application with a simple dashboard-style landing page.

## Run Without Docker

### Required Tools
- .NET 10 SDK

### Start Locally
```bash
cd Docker/example-projects/dotnet
dotnet restore
dotnet run
```

Open http://localhost:8080

## Notes
- This example is intentionally small and works well for explaining image build steps and multi-stage Dockerfiles.
- If `dotnet` is not installed, install the .NET 10 SDK first.
- The app exposes a visual page on `/`, a health endpoint on `/health`, and metadata on `/api/info`.
- `Dockerfile` is the single-stage version for teaching basics.
- `dotnet-multistage-dockerfile` is the optimized multi-stage version for image size and cleaner runtime images.

## Useful Commands
```bash
dotnet restore
dotnet run
dotnet publish -c Release
```
