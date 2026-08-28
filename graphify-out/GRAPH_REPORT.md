# Graph Report - MDMPI.App  (2026-08-28)

## Corpus Check
- 164 files · ~183,276 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 1848 nodes · 3205 edges · 108 communities (105 shown, 3 thin omitted)
- Extraction: 91% EXTRACTED · 9% INFERRED · 0% AMBIGUOUS · INFERRED: 290 edges (avg confidence: 0.84)
- Token cost: 0 input · 0 output

## Community Hubs (Navigation)
- Request Controller Tests
- PickUp Controller & Tests
- DbContext & ACCMST Mapping
- Remarks Service Stack
- Air/Sea Request Model
- Request Standard Model
- Database Schema & Triggers
- Request Standard DTO
- PullOut/Return Model
- Core DTOs & Interfaces
- Air/Sea Request DTO
- WebSocket Integration Tests
- Standard History DTO
- PullOut/Return DTO
- PullOut History Model
- Standard History Model
- Launch Settings
- Air/Sea Repository
- PostgreSQL UTC DbContext
- Air/Sea Service
- Air/Sea History DTO
- Core DTOs & Interfaces
- Core DTOs & Interfaces
- Category Feature Stack
- Air/Sea History Model
- Solution & Projects
- Air/Sea Controller & Mobile
- Air/Sea Update DTO
- Client Lookup ACCMST
- WebSocket Connection Handler
- Core DTOs & Interfaces
- Collection Transaction Controller
- PickUp History Model
- PickUp Request Model
- Image Upload Service
- Counter & Mobile Models
- PickUp Request DTO
- PullOut/Return Service
- Architecture & WebSocket Docs
- Collection Details DTO
- Item & Batch Models
- Gemini Inventory DTO
- Item Repository & ID Generators
- PullOut Insert DTO
- Request Insert DTO
- PickUp Repository Tests
- Gemini Image Analysis DTOs
- Collection Update DTO
- PullOut/Return Controller
- Backload Repository & IDs
- Request Repository
- PickUp Insert DTO
- PickUp Update DTO
- Request Update DTO
- PullOut/Return Repository
- Gemini Service
- Request Service Layer
- Backload Controller
- Collection Repository
- Request Status Filter
- PullOut Update DTO
- PickUp Repository
- WebSocket Normalizer Tests
- Item Insert DTOs
- Item Service
- PickUp Service Layer
- Item Controller
- Location Update Payload
- Item Fetch DTOs
- Air/Sea Insert DTO
- Backload Service
- Collection Module Files
- Query Filter Helper
- Item Update DTO
- Backload Model
- Collection Repository
- WebSocket Message Files
- Batch ID Generator
- Backload DTO
- WS Normalization Result
- Collection Create DTO
- Backload Insert DTO
- Transaction Helper
- WebSocket Message Normalizer
- Gemini Settings & Controller
- Item Batch Fetch DTO
- Item Batch Update DTO
- Request Query Filters
- Request Service Layer
- PullOut Controller Tests
- Core DTOs & Interfaces
- Collection Transaction Service
- Request Query Filters
- Image Path Model
- Remarks Model
- Item Repo Interface
- Gemini Image Endpoint
- Category Model
- Clean Architecture Docs
- Text Helpers
- Core DTOs & Interfaces
- Image DTO
- Signature DTO
- Counter & Mobile Models
- Counter & Mobile Models
- Image Model
- Common Project

## God Nodes (most connected - your core abstractions)
1. `MDMPI.App.Core.Common.DTOs` - 71 edges
2. `MDMPI.App.Core.Common.Interfaces` - 54 edges
3. `RequestStandardModel` - 50 edges
4. `PostgreSqlAppDbContext` - 49 edges
5. `RequestStandardDto` - 49 edges
6. `RequestPullOutReturnPickUpModel` - 45 edges
7. `RequestAirSeaDto` - 42 edges
8. `RequestAirSeaModel` - 42 edges
9. `RequestPullOutReturnPickUpDto` - 41 edges
10. `RequestQueryDto` - 40 edges

## Surprising Connections (you probably didn't know these)
- `RequestController` --references--> `IImageService`  [EXTRACTED]
  MDMPI.App.Api/Controllers/Logistic/RequestController.cs → MDMPI.App.Core/Common/Interfaces/IImageService.cs
- `RequestController` --references--> `IMobileService`  [EXTRACTED]
  MDMPI.App.Api/Controllers/Logistic/RequestController.cs → MDMPI.App.Core/Common/Interfaces/IMobileService.cs
- `RequestController` --references--> `IRemarksService`  [EXTRACTED]
  MDMPI.App.Api/Controllers/Logistic/RequestController.cs → MDMPI.App.Core/Common/Interfaces/IRemarksService.cs
- `RequestController` --references--> `IRequestService`  [EXTRACTED]
  MDMPI.App.Api/Controllers/Logistic/RequestController.cs → MDMPI.App.Core/Logistic/Interfaces/IRequestService.cs
- `PostgreSqlAppDbContext` --references--> `RequestPullOutReturnPickUpHistoryModel`  [EXTRACTED]
  MDMPI.App.Data/PostgreSqlAppDbContext.cs → MDMPI.App.Core/Logistic/Entities/RequestPullOutReturnPickUpHistoryModel.cs

## Import Cycles
- None detected.

## Hyperedges (group relationships)
- **Clean Architecture Layering (Api -> Core <- Data)** — _github_copilot_instructions_mdmpi_app_api, _github_copilot_instructions_mdmpi_app_core, _github_copilot_instructions_mdmpi_app_data, _github_copilot_instructions_clean_architecture [EXTRACTED 1.00]
- **WebSocket Real-Time Broadcast Flow** — mdmpi_app_api_websockets_readme_websocket_endpoint, mdmpi_app_api_websockets_readme_api_key_auth, mdmpi_app_api_websockets_readme_location_update_payload, mdmpi_app_api_websockets_readme_notification_payload [EXTRACTED 1.00]

## Communities (108 total, 3 thin omitted)

### Community 0 - "Request Controller Tests"
Cohesion: 0.08
Nodes (37): ActionResult, Consumes, HttpGet, HttpPatch, HttpPost, ProducesResponseType, Task, RequestController (+29 more)

### Community 1 - "PickUp Controller & Tests"
Cohesion: 0.08
Nodes (28): ActionResult, Consumes, HttpGet, HttpPatch, HttpPost, ProducesResponseType, Task, RequestPickUpController (+20 more)

### Community 2 - "DbContext & ACCMST Mapping"
Cohesion: 0.05
Nodes (37): DbContext, DateTime, CollectionTransactionDetailsModel, Amount, Bank, CheckDate, CheckNo, Client (+29 more)

### Community 3 - "Remarks Service Stack"
Cohesion: 0.10
Nodes (18): RemarksDto, Date, Remarks, RequestID, UserUpdated, Task, IRemarksService, Task (+10 more)

### Community 4 - "Air/Sea Request Model"
Cohesion: 0.05
Nodes (38): SignatureModel, RequestID, RequestReceiverSignature, DateTime, List, RequestAirSeaModel, CancelRemarks, Client (+30 more)

### Community 5 - "Request Standard Model"
Cohesion: 0.05
Nodes (37): DateOnly, DateTime, List, RequestStandardModel, Client, DocumentReference, FormCategoryID, Image (+29 more)

### Community 6 - "Database Schema & Triggers"
Cohesion: 0.06
Nodes (30): public.a_tblbackloadcounters, public.a_tblbatchcounters, public.a_tblcategory, public.a_tblitemcounters, public.a_tblmobile, public.a_tblrequestairsea, public.a_tblrequestairsea_history, public.a_tblrequestbackload (+22 more)

### Community 7 - "Request Standard DTO"
Cohesion: 0.06
Nodes (35): List, RequestStandardDto, CancelRemarks, Client, ClientID, CreatedAt, CreatedBy, DeliveredAt (+27 more)

### Community 8 - "PullOut/Return Model"
Cohesion: 0.06
Nodes (35): DateOnly, DateTime, List, RequestPullOutReturnPickUpModel, Client, ClientContactPerson, ClientID, CreatedAt (+27 more)

### Community 9 - "Core DTOs & Interfaces"
Cohesion: 0.12
Nodes (7): MDMPI.App.Core.Common.Interfaces, MDMPI.App.Core.Common.DTOs.Item, MDMPI.App.Data.Common.Repositories, MDMPI.App.Core.Common.Services, MDMPI.App.Api.Controllers.Common, MDMPI.App.Data.Common.Services, MDMPI.App.Core.Common.Entities.Item

### Community 10 - "Air/Sea Request DTO"
Cohesion: 0.06
Nodes (32): DateTime, List, RequestAirSeaDto, Client, ClientID, CreatedAt, CreatedBy, DatePickUp (+24 more)

### Community 11 - "WebSocket Integration Tests"
Cohesion: 0.14
Nodes (18): MDMPI.App.Tests, InlineData, Program, Fact, Task, Api4RouteCompatibilityTests, CancellationToken, Exception (+10 more)

### Community 12 - "Standard History DTO"
Cohesion: 0.06
Nodes (30): DateTime, RequestStandardHistoryDto, ActionType, ChangedAt, ChangedBy, FormCategoryID, HistoryID, ItemCategoryID (+22 more)

### Community 13 - "PullOut/Return DTO"
Cohesion: 0.07
Nodes (30): DateOnly, DateTime, List, RequestPullOutReturnPickUpDto, Client, ClientContactPerson, ClientID, CreatedAt (+22 more)

### Community 14 - "PullOut History Model"
Cohesion: 0.07
Nodes (30): DateTime, RequestPullOutReturnPickUpHistoryModel, ActionType, ChangedAt, ChangedBy, ClientContactPerson, ClientID, CreatedAt (+22 more)

### Community 15 - "Standard History Model"
Cohesion: 0.07
Nodes (30): DateTime, RequestStandardHistoryModel, ActionType, ChangedAt, ChangedBy, FormCategoryID, HistoryID, ItemCategoryID (+22 more)

### Community 16 - "Launch Settings"
Cohesion: 0.07
Nodes (28): ASPNETCORE_ENVIRONMENT, applicationUrl, commandName, dotnetRunMessages, environmentVariables, launchBrowser, launchUrl, applicationUrl (+20 more)

### Community 17 - "Air/Sea Repository"
Cohesion: 0.10
Nodes (18): Task, IRequestIdGenerator, Action, ILogger, PostgreSqlAppDbContext, Task, RequestIdGenerator, Action (+10 more)

### Community 18 - "PostgreSQL UTC DbContext"
Cohesion: 0.08
Nodes (24): EntityEntry, DateTime, DbSet, PostgreSqlAppDbContext, a_tblBackloadCounters, a_tblBatchCounters, a_tblCategory, a_tblItemCounters (+16 more)

### Community 19 - "Air/Sea Service"
Cohesion: 0.14
Nodes (15): RequestQueryDto, DateFilter, Page, PageSize, StatusFilter, List, Task, IRequestAirSeaRepository (+7 more)

### Community 20 - "Air/Sea History DTO"
Cohesion: 0.07
Nodes (26): DateTime, RequestAirSeaHistoryDto, ActionType, ChangedAt, ChangedBy, ClientID, CreatedAt, CreatedBy (+18 more)

### Community 21 - "Core DTOs & Interfaces"
Cohesion: 0.15
Nodes (7): MDMPI.App.Data, MDMPI.App.Core.Collection.Entities, MDMPI.App.Core.Logistic.Entities, MDMPI.App.Data.Logistic.Repositories, MDMPI.App.Data.Common, MDMPI.App.Common.Utilities, MDMPI.App.Core.Common.Entities

### Community 22 - "Core DTOs & Interfaces"
Cohesion: 0.16
Nodes (7): MDMPI.App.Core.Logistic.DTOs.RequestBackload, MDMPI.App.Api.Controllers.Logistic, MDMPI.App.Core.Logistic.Interfaces, MDMPI.App.Core.Logistic.DTOs.RequestAirSea, MDMPI.App.Api.Models, MDMPI.App.Tests.Controllers, MDMPI.App.Core.Logistic.Services

### Community 23 - "Category Feature Stack"
Cohesion: 0.10
Nodes (20): HttpGet, IActionResult, Task, CategoryController, CategoryDto, Category, ID, List (+12 more)

### Community 24 - "Air/Sea History Model"
Cohesion: 0.08
Nodes (26): DateTime, RequestAirSeaHistoryModel, ActionType, ChangedAt, ChangedBy, ClientID, CreatedAt, CreatedBy (+18 more)

### Community 25 - "Solution & Projects"
Cohesion: 0.13
Nodes (24): MDMPI.App.Api, net8.0, Microsoft.EntityFrameworkCore.Tools (9.0.9), MDMPI.App.Common, MDMPI.App.Core, Microsoft.NET.Sdk, MDMPI.App.Data, Microsoft.EntityFrameworkCore.Tools (9.0.9) (+16 more)

### Community 26 - "Air/Sea Controller & Mobile"
Cohesion: 0.19
Nodes (12): ActionResult, Consumes, HttpGet, HttpPatch, HttpPost, ProducesResponseType, Task, RequestAirSeaController (+4 more)

### Community 27 - "Air/Sea Update DTO"
Cohesion: 0.08
Nodes (24): DateTime, UpdateRequestAirSeaDto, DispatchedAt, Driver, DropOffAt, Helper, ItemPreparedAt, ItemPreparedEndAt (+16 more)

### Community 28 - "Client Lookup ACCMST"
Cohesion: 0.09
Nodes (20): ACCMSTDto, ACCMAD, ACCMBC, ACCMEM, ACCMID, ACCMNM, ACCMPH, ACCMSC (+12 more)

### Community 29 - "WebSocket Connection Handler"
Cohesion: 0.16
Nodes (16): ConcurrentDictionary, ConnectedClient, HttpContext, IConfiguration, IDisposable, CancellationToken, ILogger, Task (+8 more)

### Community 30 - "Core DTOs & Interfaces"
Cohesion: 0.13
Nodes (4): MDMPI.App.Core.Common.DTOs, MDMPI.App.Tests.Logistic, MDMPI.App.Core.Logistic.DTOs.RequestPickUp, MDMPI.App.Core.Logistic.DTOs.RequestPullOutReturnPickUp

### Community 31 - "Collection Transaction Controller"
Cohesion: 0.16
Nodes (11): MDMPI.App.Api.Controllers.Collection, ActionResult, Consumes, HttpGet, HttpPost, List, ProducesResponseType, Task (+3 more)

### Community 32 - "PickUp History Model"
Cohesion: 0.10
Nodes (21): DateTime, RequestPickUpHistoryModel, ActionType, ChangedAt, ChangedBy, ClientID, CreatedAt, CreatedBy (+13 more)

### Community 33 - "PickUp Request Model"
Cohesion: 0.10
Nodes (21): DateTime, List, RequestPickUpModel, Client, ClientID, CreatedAt, CreatedBy, DatePickUp (+13 more)

### Community 34 - "Image Upload Service"
Cohesion: 0.14
Nodes (10): Task, IImagePathTypeRepository, ImageService, ILogger, Task, ImageUploadService, ILogger, PostgreSqlAppDbContext (+2 more)

### Community 35 - "Counter & Mobile Models"
Cohesion: 0.11
Nodes (14): DocumentReferenceModel, ID, Reference, RequestID, BatchCounterModel, LastNumber, YearMonth, ItemCounterModel (+6 more)

### Community 36 - "PickUp Request DTO"
Cohesion: 0.11
Nodes (19): DateTime, List, RequestPickUpDto, Client, ClientID, CreatedAt, CreatedBy, DatePickUp (+11 more)

### Community 37 - "PullOut/Return Service"
Cohesion: 0.18
Nodes (10): List, Task, IRequestPullOutReturnPickUpRepository, List, Task, IRequestPullOutReturnPickUpService, ILogger, List (+2 more)

### Community 38 - "Architecture & WebSocket Docs"
Cohesion: 0.13
Nodes (18): AppDbContext (EF Core), Scoped DI Registration in Program.cs, Gemini AI Integration (IGeminiService), Custom ID Generators (Counter Tables), Image Upload Services (ImageService / IImageService), ItemModel, MDMPI.App.Api Project, MDMPI.App.Core Project (+10 more)

### Community 39 - "Collection Details DTO"
Cohesion: 0.11
Nodes (18): DateTime, CollectionTransactionDetailsDto, Amount, Bank, CheckDate, CheckNo, Client, ClientID (+10 more)

### Community 40 - "Item & Batch Models"
Cohesion: 0.11
Nodes (16): DateOnly, ItemBatchModel, BatchQuantity, BatchSerial, ExpiryDate, RequestItemBatchID, RequestItemID, List (+8 more)

### Community 41 - "Gemini Inventory DTO"
Cohesion: 0.15
Nodes (13): JsonConverter, Type, JsonSerializerOptions, List, InventoryItemDto, Description, ItemCode, Qty (+5 more)

### Community 42 - "Item Repository & ID Generators"
Cohesion: 0.16
Nodes (11): Task, IItemIdGenerator, ILogger, List, PostgreSqlAppDbContext, Task, ItemRepository, ILogger (+3 more)

### Community 43 - "PullOut Insert DTO"
Cohesion: 0.12
Nodes (16): DateOnly, List, InsertRequestPullOutReturnPickUpDto, ClientContactPerson, ClientID, CreatedBy, DocumentReference, FormCategoryID (+8 more)

### Community 44 - "Request Insert DTO"
Cohesion: 0.12
Nodes (17): List, InsertRequestDto, DocumentReference, FormCategoryID, ItemCategoryID, Items, RecipientContactDetails, RecipientName (+9 more)

### Community 45 - "PickUp Repository Tests"
Cohesion: 0.22
Nodes (10): conn, db, Dictionary, Fact, IEnumerable, Task, FixedIdGenerator, NoOpClientLookupRepository (+2 more)

### Community 46 - "Gemini Image Analysis DTOs"
Cohesion: 0.15
Nodes (13): GeminiApiResponse, List, GeminiAnalyzeImageRequestDto, ImageBase64, MimeType, Prompt, GeminiAnalyzeImageResponseDto, Content (+5 more)

### Community 47 - "Collection Update DTO"
Cohesion: 0.14
Nodes (13): HttpPatch, IActionResult, DateTime, UpdateCollectionTransactionDetailsDto, Amount, Bank, CheckDate, CheckNo (+5 more)

### Community 48 - "PullOut/Return Controller"
Cohesion: 0.25
Nodes (9): ActionResult, Consumes, HttpGet, HttpPatch, HttpPost, IActionResult, ProducesResponseType, Task (+1 more)

### Community 49 - "Backload Repository & IDs"
Cohesion: 0.15
Nodes (11): Task, IBackloadIdGenerator, ILogger, PostgreSqlAppDbContext, Task, BackloadIdGenerator, ILogger, List (+3 more)

### Community 50 - "Request Repository"
Cohesion: 0.19
Nodes (9): Action, DateTime, Func, IEnumerable, ILogger, List, PostgreSqlAppDbContext, Task (+1 more)

### Community 51 - "PickUp Insert DTO"
Cohesion: 0.14
Nodes (12): DateTime, List, InsertRequestPickUpDto, ClientID, CreatedBy, DatePickUp, DocumentReference, ItemCategoryID (+4 more)

### Community 52 - "PickUp Update DTO"
Cohesion: 0.12
Nodes (15): DateTime, List, UpdateRequestPickUpDto, ClientID, DatePickUp, DocumentReference, ItemCategoryID, ItemPreparedAt (+7 more)

### Community 53 - "Request Update DTO"
Cohesion: 0.12
Nodes (16): UpdateRequestDto, LocationEndAt, LocationStartedAt, MobileID, Receiver, RequestDeliveredAt, RequestDeliveredBy, RequestDeliveredEndAt (+8 more)

### Community 54 - "PullOut/Return Repository"
Cohesion: 0.21
Nodes (9): Action, DateTime, Func, IEnumerable, ILogger, List, PostgreSqlAppDbContext, Task (+1 more)

### Community 55 - "Gemini Service"
Cohesion: 0.14
Nodes (15): GeminiCandidate, GeminiContent, GeminiPart, HttpClient, JsonSerializerOptions, List, GeminiApiResponse, Candidates (+7 more)

### Community 56 - "Request Service Layer"
Cohesion: 0.25
Nodes (7): List, Task, IRequestRepository, ILogger, List, Task, RequestService

### Community 57 - "Backload Controller"
Cohesion: 0.19
Nodes (10): ActionResult, HttpGet, HttpPost, IActionResult, ILogger, Task, RequestBackloadController, List (+2 more)

### Community 58 - "Collection Repository"
Cohesion: 0.23
Nodes (7): List, Task, ICollectionTransactionDetailsRepository, ILogger, List, Task, CollectionTransactionService

### Community 59 - "Request Status Filter"
Cohesion: 0.14
Nodes (14): RequestStatusFilter, All, Cancelled, Delivered, EndorsedToGuard, ForDelivery, GettingsSupliesReady, InTransit (+6 more)

### Community 60 - "PullOut Update DTO"
Cohesion: 0.14
Nodes (14): DateTime, UpdateRequestPullOutReturnPickUpDto, ClientContactPerson, Driver, Helper, MobileID, PullOutDateEndAt, PullOutDateStartAt (+6 more)

### Community 61 - "PickUp Repository"
Cohesion: 0.22
Nodes (9): Action, DateTime, Func, IEnumerable, ILogger, List, PostgreSqlAppDbContext, Task (+1 more)

### Community 62 - "WebSocket Normalizer Tests"
Cohesion: 0.29
Nodes (6): WebSocketEnvelope, LocationUpdate, Message, NotificationUpdate, Fact, WebSocketMessageNormalizerTests

### Community 63 - "Item Insert DTOs"
Cohesion: 0.15
Nodes (11): InsertItemBatchDto, BatchQuantity, BatchSerial, ExpiryDate, List, InsertItemDto, Batch, Description (+3 more)

### Community 64 - "Item Service"
Cohesion: 0.29
Nodes (7): List, Task, IItemService, ILogger, List, Task, ItemService

### Community 65 - "PickUp Service Layer"
Cohesion: 0.24
Nodes (7): List, Task, IRequestPickUpRepository, ILogger, List, Task, RequestPickUpService

### Community 66 - "Item Controller"
Cohesion: 0.26
Nodes (9): ControllerBase, HttpPut, ActionResult, HttpGet, HttpPost, ILogger, List, Task (+1 more)

### Community 67 - "Location Update Payload"
Cohesion: 0.17
Nodes (12): LocationUpdate, Client, Distance, ETA, Latitude, Longitude, RequestID, RiderId (+4 more)

### Community 68 - "Item Fetch DTOs"
Cohesion: 0.18
Nodes (11): FetchItemBatchDto, List, FetchItemDto, Batch, BatchCount, Description, ItemCode, Qty (+3 more)

### Community 69 - "Air/Sea Insert DTO"
Cohesion: 0.18
Nodes (10): DateTime, List, InsertRequestAirSeaDto, ClientID, CreatedBy, DatePickUp, DocumentReference, ItemCategoryID (+2 more)

### Community 70 - "Backload Service"
Cohesion: 0.25
Nodes (7): List, Task, IRequestBackloadRepository, ILogger, List, Task, RequestBackloadService

### Community 71 - "Collection Module Files"
Cohesion: 0.24
Nodes (4): MDMPI.App.Data.Collection.Repositories, MDMPI.App.Core.Collection.Interfaces, MDMPI.App.Core.Collection.Services, MDMPI.App.Core.Collection.Dtos

### Community 72 - "Query Filter Helper"
Cohesion: 0.27
Nodes (5): Expression, IQueryable, DateOnly, Func, QueryFilterHelper

### Community 73 - "Item Update DTO"
Cohesion: 0.20
Nodes (10): List, UpdateItemDto, Batch, Description, ItemCode, Qty, RequestID, RequestItemID (+2 more)

### Community 74 - "Backload Model"
Cohesion: 0.20
Nodes (9): DateOnly, DateTime, RequestBackloadModel, BackLoadID, DateReported, DeliveryDate, Remarks, Request (+1 more)

### Community 75 - "Collection Repository"
Cohesion: 0.33
Nodes (5): AppDbContext, ILogger, List, Task, CollectionRepository

### Community 76 - "WebSocket Message Files"
Cohesion: 0.22
Nodes (5): MDMPI.App.Api.WebSockets, MDMPI.App.Tests.WebSockets, NotificationUpdate, Body, Title

### Community 77 - "Batch ID Generator"
Cohesion: 0.25
Nodes (6): Task, IBatchIdGenerator, ILogger, PostgreSqlAppDbContext, Task, BatchIdGenerator

### Community 78 - "Backload DTO"
Cohesion: 0.22
Nodes (8): DateOnly, DateTime, RequestBackloadDto, BackLoadID, DateReported, DeliveryDate, Remarks, RequestID

### Community 79 - "WS Normalization Result"
Cohesion: 0.25
Nodes (5): WebSocketMessageNormalizationResult, InvalidJson, MessageType, NormalizedJson, Success

### Community 80 - "Collection Create DTO"
Cohesion: 0.25
Nodes (8): DateTime, CreateCollectionTransactionDetailsDto, ClientID, CollectionDate, CollectorID, ReferenceCode, Status, VisitType

### Community 81 - "Backload Insert DTO"
Cohesion: 0.25
Nodes (7): DateOnly, DateTime, InsertRequestBackloadDto, DateReported, DeliveryDate, Remarks, RequestID

### Community 82 - "Transaction Helper"
Cohesion: 0.29
Nodes (5): IDbContextTransaction, Exception, ILogger, Task, TransactionHelper

### Community 83 - "WebSocket Message Normalizer"
Cohesion: 0.57
Nodes (3): JsonElement, JsonSerializerOptions, WebSocketMessageNormalizer

### Community 84 - "Gemini Settings & Controller"
Cohesion: 0.29
Nodes (6): GeminiController, IGeminiService, GeminiSettings, ApiKey, Model, Prompt

### Community 85 - "Item Batch Fetch DTO"
Cohesion: 0.29
Nodes (6): FetchItemBatchDto, BatchQuantity, BatchSerial, ExpiryDate, RequestItemBatchID, RequestItemID

### Community 86 - "Item Batch Update DTO"
Cohesion: 0.29
Nodes (6): UpdateItemBatchDto, BatchQuantity, BatchSerial, ExpiryDate, RequestItemBatchID, RequestItemID

### Community 87 - "Request Query Filters"
Cohesion: 0.29
Nodes (7): RequestDateFilter, All, FiveDaysAgo, ThirtyDaysAgo, Today, Tomorrow, Yesterday

### Community 88 - "Request Service Layer"
Cohesion: 0.48
Nodes (3): List, Task, IRequestService

### Community 89 - "PullOut Controller Tests"
Cohesion: 0.38
Nodes (5): BadRequestObjectResult, Fact, OkObjectResult, Task, RequestPullOutReturnPickUpControllerTests

### Community 91 - "Collection Transaction Service"
Cohesion: 0.40
Nodes (3): List, Task, ICollectionTransactionService

### Community 92 - "Request Query Filters"
Cohesion: 0.33
Nodes (5): CollectionStatusFilter, All, Completed, Overdue, Pending

### Community 93 - "Image Path Model"
Cohesion: 0.33
Nodes (5): ImagePathModel, ID, ImagePath, ImageType, RequestID

### Community 94 - "Remarks Model"
Cohesion: 0.33
Nodes (6): DateTime, RemarksModel, Date, Remarks, RequestID, UserUpdated

### Community 95 - "Item Repo Interface"
Cohesion: 0.60
Nodes (3): List, Task, IItemRepository

### Community 96 - "Gemini Image Endpoint"
Cohesion: 0.40
Nodes (4): ActionResult, HttpPost, IFormFile, Task

### Community 97 - "Category Model"
Cohesion: 0.40
Nodes (4): CategoryModel, Category, ID, Type

### Community 98 - "Clean Architecture Docs"
Cohesion: 0.50
Nodes (4): Clean Architecture Pattern, Domain-First Folder Structure, MDMPI.App Solution, MDMPI.App Naming Conventions

### Community 100 - "Core DTOs & Interfaces"
Cohesion: 0.50
Nodes (3): DocumentReferenceDto, Reference, RequestID

### Community 101 - "Image DTO"
Cohesion: 0.50
Nodes (3): ImageDto, Image, RequestID

### Community 102 - "Signature DTO"
Cohesion: 0.50
Nodes (3): SignatureDto, Image, RequestID

### Community 103 - "Counter & Mobile Models"
Cohesion: 0.50
Nodes (3): BackloadCounterModel, LastNumber, YearMonth

### Community 104 - "Counter & Mobile Models"
Cohesion: 0.50
Nodes (3): MobileModel, MobileID, MobileName

### Community 105 - "Image Model"
Cohesion: 0.67
Nodes (3): ImageModel, RequestID, RequestImage

## Knowledge Gaps
- **794 isolated node(s):** `Reference`, `RequestID`, `Image`, `RequestID`, `Type` (+789 more)
  These have ≤1 connection - possible missing edges or undocumented components.
- **3 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `MDMPI.App.Core.Common.DTOs` connect `Core DTOs & Interfaces` to `PickUp Controller & Tests`, `Remarks Service Stack`, `Core DTOs & Interfaces`, `Image DTO`, `Signature DTO`, `Collection Module Files`, `Core DTOs & Interfaces`, `Gemini Inventory DTO`, `Gemini Image Analysis DTOs`, `Request Query Filters`, `Item Batch Fetch DTO`, `Core DTOs & Interfaces`, `Category Feature Stack`, `Item Batch Update DTO`, `Core DTOs & Interfaces`, `Core DTOs & Interfaces`, `Client Lookup ACCMST`?**
  _High betweenness centrality (0.169) - this node is a cross-community bridge._
- **Why does `PostgreSqlAppDbContext` connect `PostgreSQL UTC DbContext` to `DbContext & ACCMST Mapping`, `Air/Sea Request Model`, `Request Standard Model`, `PullOut/Return Model`, `PullOut History Model`, `Standard History Model`, `Air/Sea Repository`, `Core DTOs & Interfaces`, `Air/Sea History Model`, `PickUp History Model`, `PickUp Request Model`, `Counter & Mobile Models`, `Item & Batch Models`, `PickUp Repository Tests`, `Backload Model`, `Image Path Model`, `Remarks Model`, `Category Model`, `Counter & Mobile Models`, `Counter & Mobile Models`?**
  _High betweenness centrality (0.118) - this node is a cross-community bridge._
- **Why does `MDMPI.App.Core.Common.Interfaces` connect `Core DTOs & Interfaces` to `Image Upload Service`, `Item Repository & ID Generators`, `Batch ID Generator`, `Backload Repository & IDs`, `Air/Sea Repository`, `Core DTOs & Interfaces`, `Core DTOs & Interfaces`, `Core DTOs & Interfaces`, `Collection Transaction Controller`?**
  _High betweenness centrality (0.084) - this node is a cross-community bridge._
- **Are the 2 inferred relationships involving `RequestStandardDto` (e.g. with `.GetRequestAll_ReturnsOk_WhenRepositoryReturnsData()` and `.RequestController_GetRequestAll_ReturnsOk_WhenRepositoryReturnsData()`) actually correct?**
  _`RequestStandardDto` has 2 INFERRED edges - model-reasoned connections that need verification._
- **What connects `Reference`, `RequestID`, `Image` to the rest of the system?**
  _794 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `Request Controller Tests` be split into smaller, more focused modules?**
  _Cohesion score 0.07515734912995187 - nodes in this community are weakly interconnected._
- **Should `PickUp Controller & Tests` be split into smaller, more focused modules?**
  _Cohesion score 0.0824524312896406 - nodes in this community are weakly interconnected._