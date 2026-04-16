# Copilot Instructions - MDMPI.App

## Solution Overview

**MDMPI.App** is a .NET 8 Web API for logistics and collection management (deliveries, pickups, backloads, air/sea requests). It follows a **Clean Architecture** pattern:

| Project | Role |
|---------|------|
| MDMPI.App.Api | ASP.NET Core Web API - controllers, DI registration, Swagger |
| MDMPI.App.Core | Domain entities, DTOs, service interfaces and implementations |
| MDMPI.App.Data | EF Core AppDbContext, repository implementations, ID generators |
| MDMPI.App.Common | Shared utilities (helpers, text utils) |
| MDMPI.App.Tests | Unit / integration tests |

## Architecture Rules

1. **Dependency direction**: Api -> Core <- Data. Core has **no reference** to Data or Api. Data implements interfaces defined in Core.
2. **Repository pattern**: Data access is abstracted behind I*Repository interfaces in Core, implemented in Data.
3. **Service pattern**: Business logic lives in Core/Services/ classes implementing I*Service interfaces.
4. **DI registration**: All services and repositories are registered as Scoped in Program.cs unless there is an explicit reason for Singleton (e.g., ImageService).

## Naming Conventions

| Kind | Convention | Example |
|------|-----------|---------|
| Entity / Model | {Name}Model | RequestStandardModel, ItemModel |
| DTO | {Action}{Name}Dto | InsertRequestDto, UpdateItemDto, FetchItemDto |
| Repository interface | I{Name}Repository | IRequestRepository |
| Service interface | I{Name}Service | IRequestService |
| Service implementation | {Name}Service | RequestService |
| Controller | {Name}Controller | RequestController |
| DbSet property | Table name (prefixed a_tbl) | a_tblRequestStandardDelivery |

## Folder Structure

Organize by **domain area** first, then by concern:

- Core/Common/ -- shared entities, DTOs, interfaces, services
- Core/Logistic/ -- request delivery domain
- Core/Collection/ -- collection transaction domain
- Data/Common/ -- shared repositories and generators
- Data/Logistic/ -- logistic repositories
- Data/Collection/ -- collection repositories
- Api/Controllers/Common/ -- shared endpoints (Category, Item, Gemini)
- Api/Controllers/Logistic/ -- delivery/pickup/backload endpoints
- Api/Controllers/Collection/ -- collection endpoints

## Database and EF Core Guidelines

- **Target database**: SQL Server via Microsoft.Data.SqlClient.
- **DbContext**: AppDbContext in MDMPI.App.Data. Use UseSqlOutputClause(false) on tables with triggers.
- **Primary key**: Use RequestID as the primary key on Items and related tables. Do NOT introduce a separate RequestItemID unless there is a documented need for item-level identity independent of the request.
  - For multiple items per request, use a composite key (RequestID, ItemIndex) or a justified surrogate.
  - When migrating schemas that already have RequestItemID, consolidate onto RequestID.
  - **Exception**: ItemModel uses RequestItemID as its primary key — this is an intentional exception to the general "use RequestID as primary key" rule.
- **Navigation properties**: Use [ForeignKey] attributes on collection/reference navigation properties.
- **ID generation**: Custom ID generators (RequestIdGenerator, ItemIdGenerator, etc.) use counter tables. Follow the existing pattern when adding new generators.

## Coding Standards

- **Target framework**: .NET 8 (net8.0).
- **Nullable reference types**: Enabled. Mark optional properties as string?, long?, etc.
- **Async/await**: Prefer async Task<T> for all repository and service methods.
- **No magic strings**: Use constants or nameof() where possible.
- **XML doc comments**: Add <summary> on public entity properties and service methods when the purpose is not obvious.
- **Controller responses**: Return IActionResult or ActionResult<T>. Use Ok(), NotFound(), BadRequest() consistently.

## Testing

- Test project: MDMPI.App.Tests.
- Write unit tests for service classes; mock repositories via their interfaces.
- Name test methods: MethodName_Scenario_ExpectedResult.

## External Integrations

- **Gemini AI**: Configured via GeminiSettings (bound from appsettings.json). Accessed through IGeminiService.
- **Image uploads**: Handled by ImageService (singleton) and IImageService / ImageUploadService (scoped).
