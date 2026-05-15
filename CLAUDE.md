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

Hexagonal Architecture (Ports & Adapters) with four projects targeting **net10.0**:

```
src/
  Domain/                    — core hexagon: entities, value objects, domain services, output port interfaces
    Entities/                — aggregate roots and domain entities
    ValueObjects/            — immutable value types
    Services/                — domain services
    Ports/                   — output port interfaces (e.g. IRepository contracts)

  Service/                   — application layer: orchestrates use cases via ports
    UseCases/                — use case implementations
    Ports/                   — input port interfaces (use case contracts)

  Infrastructure/            — driven/secondary adapters: implement Domain output ports
    Persistence/             — EF Core DbContext and repository implementations
    Adapters/                — other driven adapters (external APIs, messaging, etc.)

  Entrypoint/                — driving/primary adapters + composition root
    Controllers/             — HTTP adapters (ASP.NET Core controllers)
    Middlewares/             — HTTP pipeline middleware
```

Dependency rule: Domain has zero project references. Service depends on Domain only. Infrastructure implements Domain ports. Entrypoint wires everything via DI.

`Entrypoint/Program.cs` is the composition root — wire DI registrations and middleware here. `appsettings.Development.json` overrides `appsettings.json` in dev.

Solution file is `PostWallAPI.slnx` (new XML solution format).
