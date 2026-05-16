# External Integrations

**Analysis Date:** 2026-05-16

## APIs & External Services

**None configured** - Current codebase contains no external API integrations. Application is self-contained with internal service layers only.

## Data Storage

**Databases:**
- SQL Server 2022 (Express edition)
  - Connection string env/config: `ConnectionStrings:Default` in `src/Entrypoint/appsettings.Development.json`
  - Client: Microsoft.EntityFrameworkCore 10.0.1 with SqlServer provider
  - DbContext: `Infrastructure.Persistence.PostWallDbContext`
  - Database name: PostWallDb
  - Credentials: Stored in appsettings.Development.json (dev only; production should use secrets)

**File Storage:**
- Local filesystem only - No cloud storage integrations detected

**Caching:**
- None - No caching layer implemented

## Authentication & Identity

**Auth Provider:**
- Custom - No external auth provider configured
- Implementation: Currently unsecured - Controllers accept requests without authentication
- Note: All endpoints in `src/Entrypoint/Controllers/NoteController.cs` lack authorization/authentication attributes

## Monitoring & Observability

**Error Tracking:**
- Built-in exception handling via `src/Entrypoint/Middlewares/ExceptionHandlingMiddleware.cs`
- Maps domain exceptions to HTTP status codes (404 NotFoundException, 409 ConflictException, 422 ValidationException)
- No external error tracking service configured (Sentry, Application Insights, etc.)

**Logs:**
- Built-in .NET logging via ILogger - Configured in appsettings.json
- Current output: Console (default ASP.NET Core behavior)
- Log levels: Information (default), Warning (Microsoft.AspNetCore)
- Usage: ExceptionHandlingMiddleware logs unhandled exceptions

## CI/CD & Deployment

**Hosting:**
- Not yet configured for production - docker-compose.yml is development-only for SQL Server
- No production deployment pipeline configured

**CI Pipeline:**
- GitHub Actions via `.github/workflows/ci.yml`
- Triggers: Push to main, pull requests to main
- Jobs:
  - Build & Test: dotnet restore, build, test on ubuntu-latest with .NET 10 preview
  - Format Check: dotnet format verification with diagnostic verbosity

## Environment Configuration

**Required env vars / connection strings:**
- `ConnectionStrings:Default` - SQL Server connection string (required for runtime)
  - Format: `Server=localhost,1433;Database=PostWallDb;User Id=sa;Password=PostWall@Dev123;TrustServerCertificate=True;`
  - Dev value hardcoded in appsettings.Development.json

**Secrets location:**
- appsettings.json files (currently storing connection string in appsettings.Development.json)
- Production should use .env or Azure Key Vault / AWS Secrets Manager (not yet implemented)
- User Secrets API available but not currently wired (typical for .NET development)

## Webhooks & Callbacks

**Incoming:**
- None - No webhook endpoints configured

**Outgoing:**
- None - No outgoing webhook integrations

## Integration Notes

**Currently Absent (Plan for Future):**
- Authentication provider (AAD, Auth0, custom JWT)
- Authorization/role-based access control (RBAC)
- External file storage (AWS S3, Azure Blob Storage)
- Error tracking (Application Insights, Sentry)
- Message queue (RabbitMQ, Azure Service Bus)
- Email/notification service
- Payment processor integration
- Observability (metrics, distributed tracing)

**Database Migrations:**
- Located: `src/Infrastructure/Migrations/`
- Tool: EF Core CLI (dotnet-ef)
- Current migration: 20260513074929_InitialCreate.cs
- Auto-migration on startup: Configured in `src/Infrastructure/DependencyInjection.cs` via `MigrateDbAsync()` called in Program.cs

---

*Integration audit: 2026-05-16*
