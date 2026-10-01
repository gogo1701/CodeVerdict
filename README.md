# CodeVerdict

CodeVerdict is an online judge platform for programming problems and contests. The solution is built with .NET 10 and separates the web experience, application use cases, business model, infrastructure, and judge engine.

## Projects

| Project | Responsibility |
| --- | --- |
| `CodeVerdict.Web` | ASP.NET Core MVC, Identity UI, and HTTP workflows |
| `CodeVerdict.Application` | Use cases, application contracts, and result/error conventions |
| `CodeVerdict.Domain` | Business concepts and rules without framework dependencies |
| `CodeVerdict.Infrastructure` | Persistence and integrations that implement application contracts |
| `CodeVerdict.Judge` | Compilation, sandbox execution, checking, and verdict production |
| `tests/*` | Automated tests grouped by the project boundary they exercise |

The intended dependency direction is `Web -> Application -> Domain`; Infrastructure implements Application/Domain-facing integrations, while Judge remains independent from the web and persistence layers.

## Prerequisites

- .NET SDK 10.0.x
- SQL Server LocalDB for the default Windows development database, or another SQL Server connection string
- Docker Desktop when sandboxed judging is implemented and enabled

## Build and Test

Run from the repository root:

```powershell
dotnet restore CodeVerdict.slnx
dotnet build CodeVerdict.slnx --no-restore
dotnet test CodeVerdict.slnx --no-build --no-restore
dotnet format CodeVerdict.slnx --verify-no-changes --no-restore
```

Run one test project with:

```powershell
dotnet test tests/CodeVerdict.Application.Tests/CodeVerdict.Application.Tests.csproj
```

## Local Development

See [docs/local-development.md](docs/local-development.md) for database configuration, migrations, secrets, and judge-runtime guidance.

## Architecture and Delivery

- [docs/project-structure.md](docs/project-structure.md) describes project boundaries and architecture.
- [docs/platform-features.md](docs/platform-features.md) describes platform capabilities.
- [docs/implementation-plan.md](docs/implementation-plan.md) tracks implementation milestones.

## Code Organization

- Domain concepts go in `Entities`, `Enums`, and `ValueObjects` under `CodeVerdict.Domain`.
- Application use cases are grouped by feature under `CodeVerdict.Application`; shared contracts and errors go in `Common`.
- EF Core, repositories, queues, and storage implementations belong under `CodeVerdict.Infrastructure`.
- Judge orchestration, sandboxing, language runners, and checkers belong under their respective folders in `CodeVerdict.Judge`.
- MVC controllers, view models, services, and filters belong in their corresponding `CodeVerdict.Web` folders.
- Tests live in `tests/` and mirror the project they verify.

Application failures should be returned as `Result<T>` with a typed `Error`, rather than thrown for expected validation, authorization, not-found, or conflict outcomes. Exceptions remain appropriate for unexpected infrastructure or programming failures and should be handled at the owning boundary.