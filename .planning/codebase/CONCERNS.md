# Concerns & Technical Debt

**Analysis Date:** 2026-05-16

## Critical Issues

### 1. Double file extension
- **File:** `src/Domain/Exceptions/ValidationException.cs.cs`
- **Problem:** Violates C# naming conventions; likely a typo when file was created
- **Impact:** Minor — file still compiles, but breaks tooling expectations
- **Fix:** Rename to `ValidationException.cs`

### 2. Orphaned code
- **File:** `Class1.cs` (default template artifact)
- **Problem:** Default-generated file with no purpose
- **Fix:** Delete

### 3. Input validation gaps
- **Location:** `CreateNoteRequest`, `UpdateNoteRequest` DTOs
- **Problem:** No validation attributes (`[Required]`, `[MaxLength]`, etc.) on inbound DTOs; domain entities accept invalid states
- **Impact:** API accepts malformed input, no 422 response for invalid data despite middleware support
- **Fix:** Add FluentValidation or DataAnnotations; enforce invariants in domain factories

### 4. Service contract mismatch
- **Location:** `NoteService.CreateAsync()`
- **Problem:** Returns `CreateNoteRequest` instead of `NoteResponse` with created entity details (ID, timestamps)
- **Impact:** Client cannot get persisted ID/timestamps in response
- **Fix:** Map created `Note` to `NoteResponse` before returning

## Data & Persistence Issues

### 5. N+1 query problem
- **Location:** Note repository queries
- **Problem:** Folder relationship not eager-loaded; each note fetch triggers extra folder query
- **Fix:** Use `.Include(n => n.Folder)` where folder data needed

### 6. Soft delete inconsistency
- **Location:** Repository implementations
- **Problem:** Only `NoteRepository` implements soft-delete filtering; related entities (WallPosition, Tags) use CASCADE instead
- **Impact:** Cascade deletes hard-delete child rows even when parent is soft-deleted
- **Fix:** Apply soft-delete pattern uniformly via EF Core global query filters

### 7. No transaction handling
- **Problem:** Multi-step operations not wrapped in explicit transactions
- **Impact:** Partial failure leaves inconsistent state
- **Fix:** Wrap multi-aggregate operations in `IDbContextTransaction` or unit-of-work

### 8. CreatedAt not initialized
- **Location:** Domain entity factories
- **Problem:** Entities can be constructed without creation timestamp
- **Fix:** Set `CreatedAt = DateTime.UtcNow` in `Create()` factory methods

## Security & Infrastructure

### 9. No authentication
- **Problem:** All endpoints publicly accessible; no user context
- **Impact:** Critical for production — anyone can read/write any note
- **Fix:** Add JWT bearer auth, scope queries by user ID

### 10. Hard-coded database password
- **File:** `docker-compose.yml`
- **Problem:** `MSSQL_SA_PASSWORD` committed to git
- **Impact:** Secret exposed in version control history
- **Fix:** Move to `.env` (gitignored) or use Docker secrets

### 11. No audit trail
- **Problem:** Cannot trace who changed what (no `CreatedBy`/`ModifiedBy` fields)
- **Fix:** Add audit columns once auth is in place

### 12. Preview .NET in CI
- **Problem:** Target framework is `net10.0` (preview); production pipeline uses unstable runtime
- **Impact:** Breaking changes between previews; not production-ready
- **Fix:** Either pin to GA release once shipped, or document as experimental

## Architecture Violations

### 13. Repository returns entities
- **Location:** `NoteRepository.GetAllAsync()` returns `PagedResult<Note>` instead of DTOs
- **Problem:** Leaks domain entities through service into controllers; violates layer separation
- **Fix:** Service layer should project entities → DTOs before returning; or repository returns projections directly

### 14. Missing health checks
- **Problem:** No `/health` endpoint for orchestration/load balancer probes
- **Fix:** Add `app.MapHealthChecks("/health")` in `Program.cs`

### 15. No test infrastructure
- **Problem:** CI invokes `dotnet test` but no test projects exist; tests silently pass
- **Impact:** False sense of safety; no regression protection
- **Fix:** Bootstrap test projects (see `TESTING.md` for plan)

## Fragile Areas

- **Exception middleware** — single switch maps all exception types; new exception types easy to miss
- **DI registration** — manual `AddScoped` per service in `DependencyInjection.cs`; easy to forget new services
- **EF Core configurations** — split across multiple `*Configuration.cs` files; relationships not centrally documented

---

*Concerns analysis: 2026-05-16*
