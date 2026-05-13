# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```bash
# Build
dotnet build

# Run (from repo root)
dotnet run --project src/Entrypoint

# Run with watch (auto-reload)
dotnet watch --project src/Entrypoint run

# Restore packages
dotnet restore

# Test (when tests exist)
dotnet test

# Run single test
dotnet test --filter "FullyQualifiedName~TestName"
```

Dev server: `http://localhost:5273` (http) or `https://localhost:7193` (https)  
OpenAPI endpoint in Development: `http://localhost:5273/openapi/v1.json`

## Architecture

Clean/layered architecture with four projects targeting **net10.0**:

```
src/
  Domain/        — entities, value objects, domain logic (no external deps)
  Service/       — application/use-case layer (depends on Domain)
  Infrastructure/ — DB, external services, repo implementations (depends on Domain + Service)
  Entrypoint/    — ASP.NET Core host, DI wiring, HTTP pipeline (depends on all)
```

Dependency rule: outer layers depend inward; Domain has zero project references.

`Entrypoint/Program.cs` is the composition root — wire DI registrations and middleware here. `appsettings.Development.json` overrides `appsettings.json` in dev.

Solution file is `PostWallAPI.slnx` (new XML solution format).
