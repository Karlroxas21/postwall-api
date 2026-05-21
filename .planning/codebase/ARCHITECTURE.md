# Architecture

**Analysis Date:** 2026-05-16

## Pattern

**Hexagonal Architecture (Ports & Adapters)** — also called Clean Architecture.

Four projects targeting `net10.0`, organized as concentric layers:

```
┌─────────────────────────────────────────────────────────┐
│  Entrypoint (driving adapters + composition root)       │
│  ┌───────────────────────────────────────────────────┐  │
│  │  Service (application layer / use cases)          │  │
│  │  ┌─────────────────────────────────────────────┐  │  │
│  │  │  Domain (core — entities, VOs, ports)       │  │  │
│  │  └─────────────────────────────────────────────┘  │  │
│  └───────────────────────────────────────────────────┘  │
│                                                          │
│  Infrastructure (driven adapters — implements ports)    │
└─────────────────────────────────────────────────────────┘
```

## Dependency Rule

- `Domain` — zero project references (pure core)
- `Service` — depends on `Domain` only
- `Infrastructure` — depends on `Domain` (implements its output ports)
- `Entrypoint` — depends on `Service` + `Infrastructure` (composition root)

Inner layers never know about outer layers. Outer layers depend inward.

## Layers

### Domain (`src/Domain/`)
**Purpose:** Business invariants and contracts. No framework code.

**Contains:**
- `Entities/` — aggregate roots (`Note`, `Folder`, `Tag`, `NoteTag`, `WallPosition`) and base class `Base` (audit columns: `CreatedAt`, `UpdatedAt`, `DeletedAt`)
- `ValueObjects/` — `NotePalette` (color palette), `PagedResult<T>`
- `Ports/` — output port interfaces (`INoteRepository`)
- `Exceptions/` — `DomainException`, `NotFoundException`, `ConflictException`, `ValidationException`
- `Services/` — domain services (currently empty)

**Patterns:**
- Entities use private setters with static `Create()` factories
- Mutation via explicit methods (`Update()`, `SoftDelete()`)
- Soft delete via `DeletedAt` timestamp

### Service (`src/Service/`)
**Purpose:** Application use cases — orchestrate domain via ports.

**Contains:**
- `UseCases/` — `NoteService` (CRUD orchestration)
- `Ports/` — input port interfaces (`INoteService`) consumed by controllers
- `Dtos/` — `CreateNoteRequest`, `UpdateNoteRequest`, `NoteResponse` (records)
- `DependencyInjection.cs` — `AddService()` extension

**Patterns:**
- One service per aggregate
- Maps domain entities → DTOs (entities never leak to controllers — *partially violated; see CONCERNS.md*)
- Throws domain exceptions; no direct HTTP awareness

### Infrastructure (`src/Infrastructure/`)
**Purpose:** Driven adapters — implements domain output ports.

**Contains:**
- `Persistence/PostWallDbContext.cs` — EF Core DbContext
- `Persistence/NoteRepository.cs` — implements `INoteRepository`
- `Persistence/Configurations/` — Fluent API entity configs (one per entity)
- `Migrations/` — EF Core migrations (initial: `20260513074929_InitialCreate`)
- `Adapters/` — placeholder for other driven adapters (currently empty)
- `DependencyInjection.cs` — `AddInfrastructure()` + `MigrateDbAsync()`

**Patterns:**
- Repository per aggregate root
- Soft-delete filtering inline (`Where(n => n.DeletedAt == null)`)
- DbContext registered scoped via `UseSqlServer` with `ConnectionStrings:Default`

### Entrypoint (`src/Entrypoint/`)
**Purpose:** Primary/driving adapters + composition root.

**Contains:**
- `Program.cs` — DI wiring, middleware pipeline, route mapping
- `Controllers/NoteController.cs` — ASP.NET MVC controller (`[ApiController]`)
- `Middlewares/ExceptionHandlingMiddleware.cs` — exception → HTTP status mapping
- `appsettings.json` / `appsettings.Development.json` — config

**Patterns:**
- One controller per aggregate route prefix (`v1/api/notes`)
- Controllers thin — delegate to service immediately
- Composition root calls `AddInfrastructure()` then `AddService()` then `AddControllers()`

## Data Flow (CRUD)

**Example — Create Note:**

```
HTTP POST /v1/api/notes  { title, content, ... }
  │
  ▼
NoteController.Create (Entrypoint)
  │  CreateNoteRequest (DTO)
  ▼
INoteService.CreateAsync → NoteService.CreateAsync (Service)
  │  Note.Create(...) factory
  │  Domain.Entities.Note
  ▼
INoteRepository.AddAsync → NoteRepository.AddAsync (Infrastructure)
  │  _db.Notes.AddAsync + SaveChangesAsync
  ▼
SQL Server (PostWallDb)
```

**Read flow:** Controller → Service → Repository → DbContext → SQL → Entity → DTO projection → JSON response.

## Entry Points

**HTTP:**
- `http://localhost:5273` (dev HTTP)
- `https://localhost:7193` (dev HTTPS)
- OpenAPI: `http://localhost:5273/openapi/v1.json` (Development only)

**Composition root:** `src/Entrypoint/Program.cs`

**Pipeline order:**
1. `MapOpenApi()` (dev only)
2. `UseMiddleware<ExceptionHandlingMiddleware>()`
3. `UseHttpsRedirection()`
4. `MapControllers()`
5. `MigrateDbAsync()` runs migrations on startup

## Error Handling

Centralized in `ExceptionHandlingMiddleware`:

| Exception type | HTTP status |
|----------------|-------------|
| `NotFoundException` | 404 Not Found |
| `ConflictException` | 409 Conflict |
| `ValidationException` | 422 Unprocessable Entity |
| (default) | 500 Internal Server Error |

Returns RFC 7807 `application/problem+json` response. Logs all unhandled exceptions via `ILogger<ExceptionHandlingMiddleware>`.

## Abstractions

**Output ports (Domain → Infrastructure):**
- `INoteRepository` — CRUD on `Note` aggregate

**Input ports (Service → Entrypoint):**
- `INoteService` — note use cases

**No direct abstractions** for: configuration (uses `IConfiguration` directly in `AddInfrastructure`), logging (uses `ILogger<T>` directly in middleware).

## Migration & Bootstrap

- `MigrateDbAsync()` invoked on `Program.cs` startup — auto-applies pending EF migrations
- Initial schema: `Notes`, `Folders`, `Tags`, `NoteTags`, `WallPositions`
- Single `PostWallDbContextModelSnapshot` tracks model state

## What Lives Where (Quick Reference)

| Need to add... | Go to |
|----------------|-------|
| New domain entity | `src/Domain/Entities/` + create EF config in `src/Infrastructure/Persistence/Configurations/` |
| New use case | `src/Service/UseCases/` + port in `src/Service/Ports/` + register in `Service/DependencyInjection.cs` |
| New repository | Port in `src/Domain/Ports/` + impl in `src/Infrastructure/Persistence/` + register in `Infrastructure/DependencyInjection.cs` |
| New API endpoint | `src/Entrypoint/Controllers/` |
| New middleware | `src/Entrypoint/Middlewares/` + register in `Program.cs` |
| New exception type | `src/Domain/Exceptions/` + add case to `ExceptionHandlingMiddleware` switch |

---

*Architecture analysis: 2026-05-16*
