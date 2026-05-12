using MDMPI.App.Core.Common.DTOs;
using MDMPI.App.Core.Common.Entities;
using MDMPI.App.Core.Common.Interfaces;
using MDMPI.App.Core.Common.Services;
using MDMPI.App.Core.Logistic.DTOs.RequestPickUp;
using MDMPI.App.Core.Logistic.Entities;
using MDMPI.App.Core.Logistic.Interfaces;
using MDMPI.App.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MDMPI.App.Data.Logistic.Repositories
{
    public class RequestPickUpRepository : IRequestPickUpRepository
    {
        private readonly PostgreSqlAppDbContext _db;
        private readonly IClientLookupRepository _clientLookupRepository;
        private readonly ILogger<RequestPickUpRepository> _logger;
        private readonly IRequestIdGenerator _requestIdGenerator;

        public RequestPickUpRepository(
            PostgreSqlAppDbContext db,
            IClientLookupRepository clientLookupRepository,
            ILogger<RequestPickUpRepository> logger,
            IRequestIdGenerator requestIdGenerator)
        {
            _db = db;
            _clientLookupRepository = clientLookupRepository;
            _logger = logger;
            _requestIdGenerator = requestIdGenerator;
        }

        public async Task<List<RequestPickUpDto>> GetAllAsync(RequestQueryDto query)
        {
            _logger.LogInformation("Fetching all pickup requests.");

            var requests = _db.a_tblRequestPickUpMDMPI.AsNoTracking();

            // Apply date filter on DatePickUp (translateable to SQL)
            if (query.DateFilter != RequestDateFilter.All)
            {
                var today = NormalizeToUtc(DateTime.UtcNow.Date);
                DateTime start;
                DateTime end;
                switch (query.DateFilter)
                {
                    case RequestDateFilter.Today:
                        start = today;
                        end = today.AddDays(1);
                        break;
                    case RequestDateFilter.Yesterday:
                        start = today.AddDays(-1);
                        end = today;
                        break;
                    case RequestDateFilter.Tomorrow:
                        start = today.AddDays(1);
                        end = today.AddDays(2);
                        break;
                    case RequestDateFilter.FiveDaysAgo:
                        start = today.AddDays(-5);
                        end = today.AddDays(-4);
                        break;
                    case RequestDateFilter.ThirtyDaysAgo:
                        start = today.AddDays(-30);
                        end = today.AddDays(-29);
                        break;
                    default:
                        start = DateTime.MinValue;
                        end = DateTime.MaxValue;
                        break;
                }
                requests = requests.Where(r => r.DatePickUp >= start && r.DatePickUp < end);
            }

            // Apply status filter
            if (query.StatusFilter != RequestStatusFilter.All)
            {
                string? statusValue = query.StatusFilter switch
                {
                    RequestStatusFilter.NewRequest => "New Request",
                    RequestStatusFilter.GettingsSupliesReady => "Getting Supplies Ready",
                    RequestStatusFilter.ItemPacked => "Item Packed",
                    RequestStatusFilter.ForDelivery => "For Delivery",
                    RequestStatusFilter.InTransit => "In Transit",
                    RequestStatusFilter.Delivered => "Delivered",
                    RequestStatusFilter.Received => "Received",
                    RequestStatusFilter.Cancelled => "Cancelled",
                    _ => null
                };

                if (!string.IsNullOrWhiteSpace(statusValue))
                {
                    requests = requests.Where(r => r.Status == statusValue);
                }
            }

            var result = await requests
                .Select(r => new RequestPickUpDto
                {
                    RequestID = r.RequestID,
                    ClientID = r.ClientID,
                    ItemCategoryID = r.ItemCategoryID,
                    DocumentReference = _db.a_tblRequestDocumentReference
                        .Where(dr => dr.RequestID == r.RequestID)
                        .Select(dr => dr.Reference!)
                        .ToList(),
                    PreparedBy = r.PreparedBy,
                    ItemPreparedAt = r.ItemPreparedAt,
                    ItemPreparedEndAt = r.ItemPreparedEndAt,
                    DatePickUp = r.DatePickUp,
                    Remarks = r.Remarks,
                    Status = r.Status,
                    ReleasedBy = r.ReleasedBy,
                    ReceivedBy = r.ReceivedBy,
                    CreatedBy = r.CreatedBy,
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt,
                })
                .ToListAsync();

            await PopulateClientsAsync(result, item => item.ClientID, (item, client) => item.Client = client);

            _logger.LogInformation("Fetched {Count} pickup requests.", result.Count);
            return result;
        }

        public async Task<RequestPickUpDto?> InsertAsync(InsertRequestPickUpDto dto)
        {
            using var transaction = await _db.Database.BeginTransactionAsync();

            try
            {
                _logger.LogInformation("Inserting new pickup request for ClientID: {ClientID}", dto.ClientID);

                var newRequestId = await _requestIdGenerator.GenerateAsync();

                var entity = new RequestPickUpModel
                {
                    RequestID = newRequestId,
                    ClientID = dto.ClientID,
                    ItemCategoryID = dto.ItemCategoryID,
                    DocumentReference = null, // stored separately
                    DatePickUp = NormalizeNullableUtc(dto.DatePickUp),
                    Status = string.IsNullOrWhiteSpace(dto.Status) ? "New Request" : dto.Status,
                    CreatedBy = dto.CreatedBy,
                    CreatedAt = DateTime.UtcNow
                };

                _db.a_tblRequestPickUpMDMPI.Add(entity);
                await _db.SaveChangesAsync();

                // Persist document references if provided
                if (dto.DocumentReference is { Count: > 0 })
                {
                    var refs = dto.DocumentReference.Select(dr => new DocumentReferenceModel
                    {
                        RequestID = entity.RequestID,
                        Reference = dr
                    });
                    _db.a_tblRequestDocumentReference.AddRange(refs);
                    await _db.SaveChangesAsync();
                }

                await transaction.CommitAsync();
                _logger.LogInformation("Inserted pickup request with ID: {RequestID}", entity.RequestID);

                var inserted = await _db.a_tblRequestPickUpMDMPI
                    .AsNoTracking()
                    .Where(r => r.RequestID == entity.RequestID)
                    .Select(r => new RequestPickUpDto
                    {
                        RequestID = r.RequestID,
                        ClientID = r.ClientID,
                        ItemCategoryID = r.ItemCategoryID,
                        DocumentReference = _db.a_tblRequestDocumentReference
                            .Where(dr => dr.RequestID == r.RequestID)
                            .Select(dr => dr.Reference!)
                            .ToList(),
                        PreparedBy = r.PreparedBy,
                        ItemPreparedAt = r.ItemPreparedAt,
                        ItemPreparedEndAt = r.ItemPreparedEndAt,
                        DatePickUp = r.DatePickUp,
                        Remarks = r.Remarks,
                        Status = r.Status,
                        ReleasedBy = r.ReleasedBy,
                        ReceivedBy = r.ReceivedBy,
                        CreatedBy = r.CreatedBy,
                        CreatedAt = r.CreatedAt,
                        UpdatedAt = r.UpdatedAt,
                    })
                    .FirstOrDefaultAsync();

                if (inserted != null)
                {
                    await PopulateClientsAsync(new[] { inserted }, item => item.ClientID, (item, client) => item.Client = client);
                }

                return inserted;
            }
            catch (Exception ex)
            {
                await TransactionHelper.RollbackTransactionAsync(transaction, _logger, ex, "inserting PickUp request");
                return null;
            }
        }

        public async Task<bool> UpdateAsync(UpdateRequestPickUpDto dto)
        {
            using var transaction = await _db.Database.BeginTransactionAsync();

            try
            {
                _logger.LogInformation("Updating pickup request with ID: {RequestID}", dto.RequestID);

                var entity = await _db.a_tblRequestPickUpMDMPI
                    .FirstOrDefaultAsync(x => x.RequestID == dto.RequestID);

                if (entity is null)
                {
                    _logger.LogWarning("Pickup request with ID: {RequestID} not found.", dto.RequestID);
                    return false;
                }

                // Always allowed updates when provided
                QueryFilterHelper.UpdateIfNotNull(v => entity.PreparedBy = v, dto.PreparedBy);
                if (dto.ItemPreparedAt.HasValue)
                {
                    entity.ItemPreparedAt = NormalizeToUtc(dto.ItemPreparedAt.Value);
                }

                // Handle status change and UpdatedAt
                var incomingStatus = dto.Status;
                if (!string.IsNullOrWhiteSpace(incomingStatus) && !string.Equals(incomingStatus, entity.Status, StringComparison.Ordinal))
                {
                    entity.Status = incomingStatus;
                    entity.UpdatedAt = DateTime.UtcNow; // UpdatedAt on every change of status
                }

                // Determine effective status (incoming if provided, else current)
                var effectiveStatus = !string.IsNullOrWhiteSpace(incomingStatus) ? incomingStatus! : entity.Status;

                // Getting Supplies Ready: only PreparedBy and ItemPreparedAt (already handled above)
                // ItemPreparedEndAt only if status is Item Packed or Item Prepared
                if (string.Equals(effectiveStatus, "Item Packed", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(effectiveStatus, "Item Prepared", StringComparison.OrdinalIgnoreCase))
                {
                    if (dto.ItemPreparedEndAt.HasValue)
                    {
                        entity.ItemPreparedEndAt = NormalizeToUtc(dto.ItemPreparedEndAt.Value);
                    }
                }

                // Remarks and ReceivedBy only if status is Delivered or Received
                if (string.Equals(effectiveStatus, "Delivered", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(effectiveStatus, "Received", StringComparison.OrdinalIgnoreCase))
                {
                    QueryFilterHelper.UpdateIfNotNull(v => entity.Remarks = v, dto.Remarks);
                    QueryFilterHelper.UpdateIfNotNull(v => entity.ReceivedBy = v, dto.ReceivedBy);
                }

                // Replace document references if provided
                if (dto.DocumentReference is { Count: > 0 })
                {
                    var existing = _db.a_tblRequestDocumentReference.Where(dr => dr.RequestID == entity.RequestID);
                    _db.a_tblRequestDocumentReference.RemoveRange(existing);
                    await _db.SaveChangesAsync();

                    var newRefs = dto.DocumentReference.Select(dr => new DocumentReferenceModel
                    {
                        RequestID = entity.RequestID,
                        Reference = dr
                    });
                    _db.a_tblRequestDocumentReference.AddRange(newRefs);
                }

                // Update PickUpDate with proper UTC parsing
                if (dto.DatePickUp.HasValue)
                {
                    entity.DatePickUp = NormalizeToUtc(dto.DatePickUp.Value);
                }

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Updated pickup request with ID: {RequestID}", dto.RequestID);
                return true;
            }
            catch (Exception ex)
            {
                await TransactionHelper.RollbackTransactionAsync(transaction, _logger, ex, "updating PickUp request");
                return false;
            }
        }

        private async Task PopulateClientsAsync<T>(IEnumerable<T> items, Func<T, string?> clientIdSelector, Action<T, ACCMSTDto> clientSetter)
        {
            var materializedItems = items.ToList();
            if (materializedItems.Count == 0)
            {
                return;
            }

            var clients = await _clientLookupRepository.GetByIdsAsync(
                materializedItems
                    .Select(clientIdSelector)
                    .OfType<string>());

            foreach (var item in materializedItems)
            {
                var clientId = clientIdSelector(item);
                if (string.IsNullOrWhiteSpace(clientId))
                {
                    continue;
                }

                if (clients.TryGetValue(clientId, out var client))
                {
                    clientSetter(item, client);
                }
            }
        }

        private static DateTime? NormalizeNullableUtc(DateTime? value)
        {
            return value.HasValue ? NormalizeToUtc(value.Value) : null;
        }

        private static DateTime NormalizeToUtc(DateTime value)
        {
            return value.Kind switch
            {
                DateTimeKind.Utc => value,
                DateTimeKind.Local => value.ToUniversalTime(),
                _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
            };
        }
    }
}
