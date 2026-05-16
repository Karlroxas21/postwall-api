# Testing Patterns

**Analysis Date:** 2026-05-16

## Test Framework

**Status:** No test projects detected in codebase

**Runner:**
- CI pipeline configured in `.github/workflows/ci.yml` with `dotnet test` command
- Configuration: `dotnet test PostWallAPI.slnx --no-build --configuration Release --verbosity normal`
- No test projects (`.Test.csproj`, `.Tests.csproj`) currently exist

**Assertion Library:**
- Not applicable - no test implementations present

**Run Commands:**
```bash
make test              # Run all tests (via Makefile)
dotnet test            # Run all tests (direct)
```

## Test File Organization

**Location:**
- No test directories present in codebase
- Recommended location would be parallel to source: `tests/Domain.Tests/`, `tests/Service.Tests/`, `tests/Infrastructure.Tests/`

**Naming:**
- Not established (no tests present)
- Recommended: `[Feature].Test.cs` or `[Feature]Tests.cs`

## Infrastructure for Testing

**Project Structure Ready:**
- Hexagonal architecture supports testing: ports are interfaces, easy to mock
- Dependency injection via `IServiceCollection` allows test substitution
- Domain layer has no external dependencies (zero project references)

**Recommended Test Project Structure:**
```
tests/
├── Domain.Tests/              # Domain logic, entities, value objects
├── Service.Tests/             # Use case/service logic
├── Infrastructure.Tests/      # Repository, database, EF Core
└── Entrypoint.Tests/          # Controllers, middleware
```

## Mocking Patterns

**Framework Recommendation:**
- Moq (common C# choice) - interface-based mocking
- NSubstitute (alternative)

**What to Mock:**
- Port interfaces: `INoteRepository` in service tests
- Database context in repository tests
- External dependencies (API clients, file storage)

**What NOT to Mock:**
- Domain entities and value objects
- Domain exceptions
- DTOs and records

**Example Pattern (recommended):**
```csharp
// Service test setup
var mockRepository = new Mock<INoteRepository>();
mockRepository
    .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
    .ReturnsAsync(testNote);

var service = new NoteService(mockRepository.Object);
```

## Test Structure

**Recommended Patterns:**

**Unit Test Structure:**
```csharp
[TestFixture]
public class NoteServiceTests
{
    private Mock<INoteRepository> _mockRepository;
    private NoteService _service;

    [SetUp]
    public void Setup()
    {
        _mockRepository = new Mock<INoteRepository>();
        _service = new NoteService(_mockRepository.Object);
    }

    [Test]
    public async Task CreateAsync_WithValidRequest_ReturnsCreatedNote()
    {
        // Arrange
        var request = new CreateNoteRequest("Title", "Content", null, false, false, null, null);

        // Act
        var result = await _service.CreateAsync(request, CancellationToken.None);

        // Assert
        Assert.That(result.Title, Is.EqualTo("Title"));
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<Note>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
```

## Test Types

**Unit Tests:**
- Scope: Individual methods/classes
- Approach: Mock all dependencies via port interfaces
- Target: `Service.UseCases`, domain entities, value objects

**Integration Tests:**
- Scope: Service + Repository layers with real database
- Approach: Use test database (SQLite in-memory)
- Target: EF Core queries, repository logic, database constraints

**E2E/API Tests:**
- Framework: WebApplicationFactory from `Microsoft.AspNetCore.Mvc.Testing`
- Scope: Full API endpoints
- Approach: Test controller routing, middleware, error handling

## Fixtures and Factories

**Test Data (Recommended Pattern):**
```csharp
public static class NoteFixtures
{
    public static Note CreateTestNote(Guid? id = null)
    {
        return Note.Create(
            title: "Test Note",
            content: "Test Content",
            color: NotePalette.Butter.Bg,
            isPinned: false,
            isArchived: false,
            dueDate: null,
            folderId: null
        );
    }

    public static CreateNoteRequest CreateTestRequest()
    {
        return new CreateNoteRequest(
            Title: "Test Note",
            Content: "Test Content",
            Color: null,
            IsPinned: false,
            IsArchived: false,
            DueDate: null,
            FolderId: null
        );
    }
}
```

**Location:** `tests/Shared/Fixtures/` or `tests/Shared/Builders/`

## Coverage

**Requirements:** Not enforced (no test projects exist)

**Recommended Target:** 70-80% overall, 90%+ for critical paths (repositories, domain logic)

## CI/CD Test Integration

**Current Pipeline:** `.github/workflows/ci.yml`

**Test Job:**
```yaml
- name: Test
  run: dotnet test PostWallAPI.slnx --no-build --configuration Release --verbosity normal
```

**Future Enhancement:** Add coverage reporting
```yaml
- name: Test with Coverage
  run: dotnet test --collect:"XPlat Code Coverage" /p:IncludeTestAssembly=true
```

## Getting Started with Testing

**Step 1: Create test projects**
- `dotnet new nunit -n Domain.Tests` (or `mstest`, `xunit`)
- `dotnet new nunit -n Service.Tests`
- `dotnet new nunit -n Infrastructure.Tests`

**Step 2: Add test dependencies**
```xml
<ItemGroup>
    <PackageReference Include="NUnit" Version="4.x" />
    <PackageReference Include="NUnit3TestAdapter" Version="4.x" />
    <PackageReference Include="Moq" Version="4.x" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.InMemory" Version="10.0.x" />
</ItemGroup>
```

**Step 3: Add project references**
```xml
<ProjectReference Include="..\..\src\Domain\Domain.csproj" />
<ProjectReference Include="..\..\src\Service\Service.csproj" />
```

## Test Patterns by Layer

**Domain Tests:**
- Entity creation and factory methods: `Note.Create()`
- Domain methods: `Note.Update()`, `Note.SoftDelete()`
- Value objects: `NotePalette` equality and properties
- Exception scenarios

**Service Tests:**
- CRUD operations through mocked repository
- DTO mapping/projection
- Exception handling (NotFoundException, etc.)
- Business logic orchestration

**Infrastructure Tests:**
- Repository queries against test database (SQLite)
- EF Core configuration
- Soft delete filters
- Pagination logic

**Controller Tests:**
- Route mapping
- Parameter binding (FromQuery, FromBody)
- HTTP status codes
- Middleware integration

---

*Testing analysis: 2026-05-16*
