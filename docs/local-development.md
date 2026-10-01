# Local Development

## Requirements

- .NET SDK 10.0.x
- SQL Server LocalDB on Windows for the checked-in Development default, or a reachable SQL Server instance
- Docker Desktop for sandbox execution once the judge runtime is implemented

The current `CodeVerdict.Judge` project is a library scaffold. Docker is not required to build or run the baseline tests yet. Never run participant code directly on the web server or developer host; sandbox execution must be implemented and security-tested before judging untrusted submissions is enabled.

## Database Configuration

The checked-in `CodeVerdict.Web/appsettings.Development.json` contains a non-secret LocalDB connection string for local Windows development. The base `appsettings.json` contains no connection string. Override the development value with .NET user secrets when using a different local database:

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<your-local-connection-string>" --project CodeVerdict.Web
```

The Web project already has a user-secrets ID. User-secret values are stored outside the repository. Do not put passwords, production connection strings, API keys, or other credentials in committed JSON files.

For deployed environments, set the configuration key `ConnectionStrings__DefaultConnection` using the hosting platform's secret/configuration facility. Do not copy local development settings into production.

Apply the current Identity schema with the EF Core CLI:

```powershell
dotnet tool install --global dotnet-ef --version 10.0.12
dotnet ef database update --project CodeVerdict.Web
```

If `dotnet-ef` is already installed, update it to a compatible 10.0.x version instead of installing it again.

## Judge Runtime

The judge and sandbox are not implemented yet. When they are added, local judge execution will require Docker Desktop with Linux containers enabled. The judge must disable network access, restrict filesystem access, enforce CPU/memory/process/time/output limits, run without root privileges, and clean up containers after every job. Those controls are requirements for enabling judging, not optional local optimizations.

## Common Commands

```powershell
dotnet restore CodeVerdict.slnx
dotnet build CodeVerdict.slnx --no-restore
dotnet test CodeVerdict.slnx --no-build --no-restore
dotnet format CodeVerdict.slnx --verify-no-changes --no-restore
dotnet run --project CodeVerdict.Web
```

The test suites do not require a production database or Docker. The Web smoke test starts the application but does not access the database.