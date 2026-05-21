# Coding Conventions

**Analysis Date:** 2026-05-16

## Naming Patterns

**Files:**
- PascalCase for class files: `Note.cs`, `NoteService.cs`, `NoteRepository.cs`
- PascalCase for namespaces: `Domain.Entities`, `Service.UseCases`, `Infrastructure.Persistence`
- Configuration classes suffixed with `Configuration`: `NoteConfiguration.cs`, `FolderConfiguration.cs`
- Interface files prefixed with `I`: `INoteService.cs`, `INoteRepository.cs`
- Request/Response DTOs suffixed with `Request` or `Response`: `CreateNoteRequest.cs`, `NoteResponse.cs`
- Middleware suffixed with `Middleware`: `ExceptionHandlingMiddleware.cs`

**Functions:**
- PascalCase for all public methods: `GetByIdAsync()`, `CreateAsync()`, `UpdateAsync()`
- PascalCase for static factory methods: `Create()` (see `Note.Create()`)
- Private methods also PascalCase: `ToNoteReponse()`
- Async methods suffixed with `Async`: `GetAllAsync()`, `DeleteAsync()`, `AddAsync()`

**Variables:**
- camelCase for local variables and parameters: `note`, `id`, `pageSize`, `ct`
- camelCase for private fields: `_noteRepository`, `_db`

**Types:**
- PascalCase for all classes and records: `Note`, `CreateNoteRequest`, `NoteResponse`
- PascalCase for interfaces: `INoteService`, `INoteRepository`
- PascalCase for custom exceptions: `DomainException`, `NotFoundException`, `ConflictException`

## Code Style

**Formatting:**
- Enforced by `dotnet format` (built-in C# formatter)
- CI pipeline enforces format verification in `.github/workflows/ci.yml`
- Run locally with `make format` from `Makefile`
- 4-space indentation (C# default)

**Linting:**
- Nullable reference types enabled in all `.csproj` files
- No explicit EditorConfig or StyleCop configuration

## Import Organization

**Order:**
1. System/BCL imports
2. Microsoft/Framework imports
3. Domain/library imports
4. Service/infrastructure imports
5. Namespace declaration

**Example from `NoteController.cs`:**
```csharp
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Service.Dtos;
using Service.Ports;

namespace Entrypoint.Controllers;
```

## Error Handling

**Patterns:**
- Domain layer throws domain-specific exceptions: `DomainException`, `NotFoundException`, `ConflictException`, `ValidationException`
- Middleware maps exceptions to HTTP responses:
  - `NotFoundException` → 404
  - `ConflictException` → 409
  - `ValidationException` → 422
  - Others → 500

**Example from `ExceptionHandlingMiddleware.cs`:**
```csharp
var (status, title) = ex switch
{
    NotFoundException => (StatusCodes.Status404NotFound, "Not Found"),
    ConflictException => (StatusCodes.Status409Conflict, "Conflict"),
    ValidationException => (StatusCodes.Status422UnprocessableEntity, "Validation Error"),
    _ => (StatusCodes.Status500InternalServerError, "Internal Server Error"),
};
```

## Logging

**Framework:** `ILogger<T>` from `Microsoft.Extensions`

**Patterns:**
- Minimal logging; only in `ExceptionHandlingMiddleware.cs`: `logger.LogError(ex, "Unhandled exception")`
- No structured logging configuration
- Service and repository layers do not log directly

## Function Design

**Size:** Concise functions (10-30 lines typical)
- Repository methods: 5-15 lines
- Service methods: 10-25 lines
- Controller actions: 5-10 lines

**Parameters:**
- Minimal parameter count (max 3-4)
- Use records for DTOs
- Async methods include `CancellationToken ct` as final parameter

**Return Values:**
- Async methods return `Task<T>` or `Task`
- Services return DTOs (`NoteResponse`), not domain entities
- Nullable returns (`Task<Note?>`) for optional entities

## Module Design

**Dependency Injection Convention:**
- Static extension methods `AddXxx()` on `IServiceCollection`
- Located in `DependencyInjection.cs` file
- Scoped registration pattern: `services.AddScoped<IService, Service>()`

**Record Types:**
- Used for DTOs: `CreateNoteRequest`, `NoteResponse`
- Used for value objects: `NotePalette`

---

*Convention analysis: 2026-05-16*
