# Technology Stack

**Analysis Date:** 2026-05-16

## Languages

**Primary:**
- C# 12+ - All source code (.cs files) across domain, service, infrastructure, and entrypoint projects

## Runtime

**Environment:**
- .NET 10 (Preview) - Target framework net10.0 across all four projects
- ASP.NET Core 10.0.7 - Web framework for HTTP API

**Package Manager:**
- NuGet - Package and dependency management
- Lockfile: Generated via dotnet restore (project lock files and build artifacts in obj/ directories)

## Frameworks

**Core:**
- ASP.NET Core 10.0.7 (Microsoft.AspNetCore.OpenApi) - REST API framework with OpenAPI support
- Entity Framework Core 10.0.1 (Microsoft.EntityFrameworkCore, Microsoft.EntityFrameworkCore.SqlServer) - ORM and data access

**Build/Dev:**
- Microsoft.EntityFrameworkCore.Design 10.0.1 - CLI tools for EF Core migrations and scaffolding
- Microsoft.EntityFrameworkCore.Tools 10.0.1 - Package manager console tools for migrations

## Key Dependencies

**Critical:**
- Microsoft.AspNetCore.OpenApi 10.0.7 - OpenAPI/Swagger support for API documentation at `http://localhost:5273/openapi/v1.json` (development only)
- Microsoft.EntityFrameworkCore.SqlServer 10.0.1 - SQL Server provider for EF Core, required for database connectivity

**Infrastructure:**
- Microsoft.Extensions.DependencyInjection.Abstractions 10.0.0 - Dependency injection container and service registration
- Microsoft.Extensions.Hosting.Abstractions 10.0.8 - Host abstractions for background services and lifecycle management

## Configuration

**Environment:**
- Configuration via appsettings.json and appsettings.{Environment}.json
- Development config: `src/Entrypoint/appsettings.Development.json` - Contains local SQL Server connection string
- Production config: `src/Entrypoint/appsettings.json` - Base logging configuration

**Build:**
- Solution file: `PostWallAPI.slnx` (new XML solution format, not legacy .sln)
- Projects: Domain, Service, Infrastructure, Entrypoint - Four-layer architecture with explicit project dependencies
- Global usings enabled (ImplicitUsings) in all projects - Reduces boilerplate imports
- Nullable reference types enabled (Nullable: enable) - Full null-safety checking

## Platform Requirements

**Development:**
- .NET 10 SDK (preview) - Required via dotnet-version in CI
- Docker & Docker Compose - For running SQL Server 2022 container
- Visual Studio, Rider, or VS Code with C# extensions - IDE support (typical)

**Production:**
- .NET 10 runtime - Minimal hosting requirement
- SQL Server 2022 (Express or higher) - Database engine
- Deployment target: Container-based (docker-compose.yml configured for local dev, but production deployment method not yet specified)

**Development Database:**
- SQL Server 2022-latest in Docker container (`mcr.microsoft.com/mssql/server:2022-latest`)
- Image: postwall-mssql
- Port: 1433
- Database name: PostWallDb
- Data volume: mssql_data (persistent)

---

*Stack analysis: 2026-05-16*
