# Directory Structure

**Analysis Date:** 2026-05-16

## Top-Level Layout

```
postwall-api/
├── src/                          # All source code
│   ├── Domain/                   # Core hexagon (no deps)
│   ├── Service/                  # Application layer
│   ├── Infrastructure/           # Driven adapters
│   └── Entrypoint/               # Driving adapters + composition root
├── .github/workflows/            # CI pipeline
│   └── ci.yml
├── .planning/                    # GSD planning artifacts
│   └── codebase/                 # Codebase maps (this directory)
├── .claude/                      # Claude Code project config
│   └── commands/                 # Custom slash commands
├── PostWallAPI.slnx              # Solution file (new XML format)
├── Makefile                      # Common dev commands
├── docker-compose.yml            # Local SQL Server
├── CLAUDE.md                     # Project instructions for Claude
└── README.md
```

## src/Domain/

```
Domain/
├── Domain.csproj                 # Zero project references
├── Entities/
│   ├── Base.cs                   # CreatedAt/UpdatedAt/DeletedAt
│   ├── Note.cs                   # Note aggregate root
│   ├── Folder.cs
│   ├── Tag.cs
│   ├── NoteTag.cs                # Join entity
│   └── WallPosition.cs
├── ValueObjects/
│   ├── NotePalette.cs            # Color palette constants
│   └── PagedResult.cs            # Generic pagination wrapper
├── Ports/
│   └── INoteRepository.cs        # Output port — implemented by Infra
├── Exceptions/
│   ├── DomainException.cs        # Base
│   ├── NotFoundException.cs
│   ├── ConflictException.cs
│   └── ValidationException.cs.cs # ⚠ filename typo — see CONCERNS.md
├── Services/                     # Currently empty
└── Class1.cs                     # ⚠ template artifact — see CONCERNS.md
```

## src/Service/

```
Service/
├── Service.csproj                # Refs: Domain
├── DependencyInjection.cs        # AddService() extension
├── UseCases/
│   └── NoteService.cs            # Implements INoteService
├── Ports/
│   └── INoteService.cs           # Input port — consumed by controllers
└── Dtos/
    ├── CreateNoteRequest.cs      # record
    ├── UpdateNoteRequest.cs      # record
    └── NoteResponse.cs           # record
```

## src/Infrastructure/

```
Infrastructure/
├── Infrastructure.csproj         # Refs: Domain
├── DependencyInjection.cs        # AddInfrastructure() + MigrateDbAsync()
├── Persistence/
│   ├── PostWallDbContext.cs      # EF Core DbContext
│   ├── NoteRepository.cs         # Implements INoteRepository
│   └── Configurations/
│       ├── NoteConfiguration.cs
│       ├── FolderConfiguration.cs
│       ├── TagConfiguration.cs
│       ├── NoteTagConfiguration.cs
│       └── WallPositionConfiguration.cs
├── Adapters/                     # Currently empty (placeholder)
└── Migrations/
    ├── 20260513074929_InitialCreate.cs
    ├── 20260513074929_InitialCreate.Designer.cs
    └── PostWallDbContextModelSnapshot.cs
```

## src/Entrypoint/

```
Entrypoint/
├── Entrypoint.csproj             # Refs: Service, Infrastructure
├── Program.cs                    # Composition root
├── appsettings.json              # Base config
├── appsettings.Development.json  # Dev override
├── Entrypoint.http               # REST client scratchpad
├── Controllers/
│   └── NoteController.cs         # Route: v1/api/notes
├── Middlewares/
│   └── ExceptionHandlingMiddleware.cs
└── Properties/
    └── launchSettings.json       # Dev URLs (5273 http, 7193 https)
```

## Naming Conventions

### Files
| Type | Pattern | Example |
|------|---------|---------|
| Class | PascalCase | `Note.cs`, `NoteService.cs` |
| Interface | `I` + PascalCase | `INoteRepository.cs` |
| EF Config | `{Entity}Configuration.cs` | `NoteConfiguration.cs` |
| Middleware | `{Name}Middleware.cs` | `ExceptionHandlingMiddleware.cs` |
| DTO | `{Verb}{Noun}Request.cs` / `{Noun}Response.cs` | `CreateNoteRequest.cs` |
| Controller | `{Aggregate}Controller.cs` | `NoteController.cs` |
| DI module | `DependencyInjection.cs` (one per project) | — |

### Namespaces
Match folder hierarchy:
- `Domain.Entities`, `Domain.ValueObjects`, `Domain.Ports`, `Domain.Exceptions`
- `Service.UseCases`, `Service.Ports`, `Service.Dtos`
- `Infrastructure.Persistence`, `Infrastructure.Persistence.Configurations`
- `Entrypoint.Controllers`, `Entrypoint.Middlewares`

### Routes
- Versioned prefix: `v1/api/{resource}` (plural)
- Route param syntax: `{id:guid}`

## Where to Add New Code

| Task | Location |
|------|----------|
| New aggregate | `src/Domain/Entities/{Name}.cs` (extend `Base`); add EF config in `Infrastructure/Persistence/Configurations/`; add migration |
| New use case method | Add to existing `{Aggregate}Service.cs` in `src/Service/UseCases/` + interface in `Service/Ports/` |
| New repository method | Add to port `src/Domain/Ports/I{Aggregate}Repository.cs` + impl in `Infrastructure/Persistence/{Aggregate}Repository.cs` |
| New endpoint | New action in `src/Entrypoint/Controllers/{Aggregate}Controller.cs` |
| New value object | `src/Domain/ValueObjects/` |
| New domain exception | `src/Domain/Exceptions/` + add to switch in `ExceptionHandlingMiddleware` |
| New DTO | `src/Service/Dtos/` (use `record`) |
| Composition wiring | `src/Entrypoint/Program.cs` |
| Per-project DI | `src/{Project}/DependencyInjection.cs` |

## Build Artifacts (gitignored)

- `bin/` — compiled binaries
- `obj/` — intermediate build files (includes `*.AssemblyInfo.cs`, `*.GlobalUsings.g.cs`)

## Solution

- **File:** `PostWallAPI.slnx` (new XML solution format)
- **Target framework:** `net10.0` (preview — see CONCERNS.md)
- **All projects** enable nullable reference types

---

*Structure analysis: 2026-05-16*
