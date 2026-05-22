# PostWall API

A sticky-note / pinboard backend built with ASP.NET Core on **.NET 10**, organized as a Hexagonal (Ports & Adapters) architecture and backed by SQL Server via Entity Framework Core.

PostWall lets users create colored notes, organize them in folders, pin them to a virtual wall (with X/Y/width/height coordinates), and label them with tags. This repository exposes the REST API that powers those features.

---

## Table of Contents

- [Features](#features)
- [Tech Stack](#tech-stack)
- [Architecture](#architecture)
- [Project Structure](#project-structure)
- [Getting Started](#getting-started)
  - [Prerequisites](#prerequisites)
  - [Database](#database)
  - [Run the API](#run-the-api)
- [Configuration](#configuration)
- [Make Targets](#make-targets)
- [API Reference](#api-reference)
  - [Notes](#notes)
  - [Tags](#tags)
- [Domain Model](#domain-model)
- [Error Handling](#error-handling)
- [Testing](#testing)
- [Database Migrations](#database-migrations)
- [Contributing](#contributing)

---

## Features

- CRUD for **Notes** (title, content, color, pin/archive flags, due date, folder).
- CRUD for **Tags** with color and live note-count aggregation.
- Attach / detach tags to notes (many-to-many via `NoteTag`).
- **Folders** for grouping notes.
- **WallPosition** value object for placing notes on a 2D pinboard.
- Pre-defined **NotePalette** (Butter, Peach, Sage, Sky, Blush, Sand) for consistent note colors.
- **Soft delete** via `DeletedAt` timestamp.
- Cursor-friendly **paged responses** (`Page`, `PageSize`, `TotalCount`, `HasNext`, `HasPrevious`).
- RFC 7807 **ProblemDetails** error responses via a centralized exception middleware.
- OpenAPI document exposed in Development.

## Tech Stack

| Layer            | Choice                              |
| ---------------- | ----------------------------------- |
| Runtime          | .NET 10 (`net10.0`)                 |
| Web framework    | ASP.NET Core (Controllers)          |
| ORM              | Entity Framework Core (SQL Server)  |
| Database         | Microsoft SQL Server 2022           |
| API docs         | Microsoft.AspNetCore.OpenApi        |
| Test runner      | `dotnet test` (xUnit by convention) |
| Solution format  | `.slnx` (XML solution)              |
| Container (dev)  | Docker Compose                      |

## Architecture

PostWall follows **Hexagonal Architecture** (a.k.a. Ports & Adapters). The dependency rule flows inward toward the Domain:

```
            ┌──────────────────────────────┐
            │        Entrypoint            │  ← Driving adapters (HTTP)
            │  Controllers, Middleware,    │     Composition root
            │  Program.cs                  │
            └─────────────┬────────────────┘
                          │ depends on
            ┌─────────────▼────────────────┐
            │          Service             │  ← Application layer
            │  Use cases, DTOs, input      │     Orchestrates ports
            │  ports (INoteService, …)     │
            └─────────────┬────────────────┘
                          │ depends on
            ┌─────────────▼────────────────┐
            │          Domain              │  ← Core (zero references)
            │  Entities, ValueObjects,     │     Business rules
            │  Output ports (IRepository)  │
            └─────────────▲────────────────┘
                          │ implements
            ┌─────────────┴────────────────┐
            │       Infrastructure         │  ← Driven adapters
            │  EF Core DbContext, repo     │     Implements Domain ports
            │  implementations, migrations │
            └──────────────────────────────┘
```

- **Domain** has zero project references — pure business model.
- **Service** depends only on Domain. Holds use case implementations and DTOs.
- **Infrastructure** implements Domain output ports (e.g. `INoteRepository`).
- **Entrypoint** wires everything via DI in `Program.cs` and exposes the HTTP surface.

## Project Structure

```
postwall-api/
├── src/
│   ├── Domain/                       — entities, value objects, ports, exceptions
│   │   ├── Entities/                 (Note, Tag, Folder, NoteTag, WallPosition, Base)
│   │   ├── ValueObjects/             (NotePalette, PagedResult<T>, TagWithNoteCount)
│   │   ├── Ports/                    (INoteRepository, ITagRepository)
│   │   └── Exceptions/               (DomainException, NotFoundException, …)
│   ├── Service/                      — use cases + input ports + DTOs
│   │   ├── UseCases/                 (NoteService, TagService)
│   │   ├── Ports/                    (INoteService, ITagService)
│   │   └── Dtos/                     (Create/Update/Response records)
│   ├── Infrastructure/               — EF Core persistence
│   │   ├── Persistence/              (PostWallDbContext, repositories)
│   │   ├── Persistence/Configurations/  (EF Core entity configs)
│   │   └── Migrations/
│   └── Entrypoint/                   — ASP.NET Core host
│       ├── Controllers/              (NoteController, TagController)
│       ├── Middlewares/              (ExceptionHandlingMiddleware)
│       └── Program.cs                (composition root)
├── tests/
│   ├── Domain.Tests/                 (entity unit tests)
│   └── Service.Tests/                (use-case unit tests)
├── docker-compose.yml                (SQL Server 2022 dev container)
├── Makefile                          (build/run/test/db shortcuts)
├── PostWallAPI.slnx                  (solution file)
└── CLAUDE.md                         (AI assistant project notes)
```

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker](https://www.docker.com/) (for the SQL Server dev container) — or a local SQL Server instance
- `dotnet-ef` tool (only required when adding / applying migrations):
  ```bash
  dotnet tool install --global dotnet-ef
  ```

### Database

The repo ships a Compose profile that boots SQL Server 2022 Express on `localhost:1433`:

```bash
make db-up      # docker compose --profile dev up -d
make db-down    # docker compose --profile dev down
```

Default credentials (development only):

| Field        | Value                |
| ------------ | -------------------- |
| Host         | `localhost,1433`     |
| Database     | `PostWallDb`         |
| User         | `sa`                 |
| Password     | `PostWall@Dev123`    |

The connection string lives in `src/Entrypoint/appsettings.Development.json` under `ConnectionStrings:Default`.

On startup, `Program.cs` calls `MigrateDbAsync()` which applies pending EF Core migrations automatically — you do not need to run `dotnet ef database update` manually after the first boot.

### Run the API

```bash
# from repo root
dotnet restore
dotnet build
dotnet run --project src/Entrypoint
```

Or with the Makefile:

```bash
make restore
make build
make run        # one-shot
make watch      # hot reload
```

| URL                                       | Purpose                                    |
| ----------------------------------------- | ------------------------------------------ |
| `http://localhost:5273`                   | HTTP base                                  |
| `https://localhost:7193`                  | HTTPS base                                 |
| `http://localhost:5273/openapi/v1.json`   | OpenAPI document (Development environment) |

## Configuration

Settings cascade: `appsettings.json` → `appsettings.Development.json` → environment variables → command-line.

Key knobs:

| Key                          | Description                              |
| ---------------------------- | ---------------------------------------- |
| `ConnectionStrings:Default`  | SQL Server connection string             |
| `Logging:LogLevel:Default`   | Root log level (default `Information`)   |

Set the environment with `ASPNETCORE_ENVIRONMENT` (`Development` enables OpenAPI).

## Make Targets

| Target          | What it does                                                |
| --------------- | ----------------------------------------------------------- |
| `make build`    | `dotnet build PostWallAPI.slnx`                             |
| `make run`      | Run the API once                                            |
| `make watch`    | Run with hot reload                                         |
| `make restore`  | Restore NuGet packages                                      |
| `make test`     | Run the full test suite                                     |
| `make format`   | `dotnet format` on the solution                             |
| `make format-check` | Verify formatting in CI                                 |
| `make db-up`    | Start the SQL Server dev container                          |
| `make db-down`  | Stop and remove the dev container                           |
| `make migration name=Foo` | Add EF Core migration `Foo` to `src/Infrastructure` |
| `make migrate`  | Apply pending migrations                                    |

## API Reference

All routes are versioned under `/v1/api/`. Responses are `application/json`; errors are `application/problem+json` (RFC 7807).

### Notes

Base route: `/v1/api/notes`

| Method | Path                                  | Description                            | Success |
| ------ | ------------------------------------- | -------------------------------------- | ------- |
| GET    | `/`                                   | Paged list of notes                    | 200     |
| GET    | `/{id:guid}`                          | Get a note by id                       | 200     |
| POST   | `/`                                   | Create a note                          | 201     |
| PUT    | `/{id:guid}`                          | Update a note                          | 200     |
| DELETE | `/{id:guid}`                          | Delete (soft) a note                   | 204     |
| POST   | `/{noteId:guid}/tags/{tagId:guid}`    | Attach a tag to a note                 | 201     |
| DELETE | `/{noteId:guid}/tags/{tagId:guid}`    | Detach a tag from a note               | 204     |

Query params on list endpoints: `page` (default `1`), `pageSize` (default `10`).

**Create payload**:

```json
{
  "title": "Buy milk",
  "content": "2% organic",
  "color": "#f5e6a8",
  "isPinned": false,
  "isArchived": false,
  "dueDate": "2026-05-25",
  "folderId": null,
  "tagIds": ["3b9a…", "7c12…"]
}
```

**Note response**:

```json
{
  "id": "…",
  "title": "Buy milk",
  "content": "2% organic",
  "color": "#f5e6a8",
  "isPinned": false,
  "isArchived": false,
  "tags": [{ "id": "…", "name": "errands", "color": "#c8d8b8" }],
  "dueDate": "2026-05-25",
  "folderId": null,
  "createdAt": "2026-05-21T08:00:00Z",
  "updatedAt": null
}
```

### Tags

Base route: `/v1/api/tags`

| Method | Path           | Description                                     | Success |
| ------ | -------------- | ----------------------------------------------- | ------- |
| GET    | `/`            | Paged list of tags with note counts             | 200     |
| GET    | `/{id:guid}`   | Get a tag by id                                 | 200     |
| POST   | `/`            | Create a tag                                    | 201     |
| PUT    | `/{id:guid}`   | Update a tag (name, color)                      | 200     |
| DELETE | `/{id:guid}`   | Delete (soft) a tag                             | 204     |

**Create payload**:

```json
{ "name": "errands", "color": "#c8d8b8" }
```

List responses are wrapped in `PagedResult<T>`:

```json
{
  "items": [ /* … */ ],
  "page": 1,
  "pageSize": 10,
  "totalCount": 42,
  "totalPages": 5,
  "hasNext": true,
  "hasPrevious": false
}
```

## Domain Model

| Entity         | Purpose                                                                 |
| -------------- | ----------------------------------------------------------------------- |
| `Note`         | A sticky note. Holds title, content, color, pin/archive flags, due date, folder. Soft-deletable. |
| `Tag`          | Reusable label with a color.                                            |
| `NoteTag`      | Join entity for the many-to-many between `Note` and `Tag`.              |
| `Folder`       | Grouping container for notes.                                           |
| `WallPosition` | Optional 2D placement (`X`, `Y`, `Width`, `Height`) per note.           |
| `Base`         | Shared `CreatedAt` / `UpdatedAt` / `DeletedAt` timestamps.              |

Value objects:

- `NotePalette` — predefined color triplets (background, text, tag dot): Butter, Peach, Sage, Sky, Blush, Sand.
- `PagedResult<T>` — generic page envelope with derived `TotalPages`, `HasNext`, `HasPrevious`.
- `TagWithNoteCount` — `(Tag, NoteCount)` projection for list endpoints.

## Error Handling

`ExceptionHandlingMiddleware` maps domain exceptions to HTTP status codes and emits an RFC 7807 `ProblemDetails` body:

| Exception              | HTTP Status                  |
| ---------------------- | ---------------------------- |
| `NotFoundException`    | 404 Not Found                |
| `ConflictException`    | 409 Conflict                 |
| `ValidationException`  | 422 Unprocessable Entity     |
| _anything else_        | 500 Internal Server Error    |

Example error body:

```json
{
  "status": 404,
  "title": "Not Found",
  "detail": "Note with id '…' was not found."
}
```

## Testing

```bash
dotnet test                              # all tests
dotnet test --filter "FullyQualifiedName~NoteServiceTests"
make test
```

Test projects:

- `tests/Domain.Tests` — entity invariants (`NoteTests`, `TagTests`).
- `tests/Service.Tests` — use-case behavior (`NoteServiceTests`, `TagServiceTests`).

## Database Migrations

Add a new migration:

```bash
make migration name=AddNoteReminder
# equivalent to:
dotnet ef migrations add AddNoteReminder \
  --project src/Infrastructure \
  --startup-project src/Entrypoint
```

Apply migrations explicitly (rarely needed — the host runs them on startup):

```bash
make migrate
```

## Contributing

1. Branch from `main` (`feat/<short-slug>` or `fix/<short-slug>`).
2. Keep the dependency rule: Domain has zero project references.
3. Run `make format` and `make test` before pushing.
4. Open a PR; CI runs `format-check` and the test suite.

---

_Solution: `PostWallAPI.slnx` · Target framework: `net10.0`_
