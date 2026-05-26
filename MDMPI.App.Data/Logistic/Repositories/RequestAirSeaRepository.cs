using MDMPI.App.Common.Utilities;
using MDMPI.App.Core.Common.DTOs;
using MDMPI.App.Core.Common.Entities;
using MDMPI.App.Core.Common.Interfaces;
using MDMPI.App.Core.Common.Services;
using MDMPI.App.Data.Common;
using MDMPI.App.Core.Logistic.DTOs.RequestAirSea;
using MDMPI.App.Core.Logistic.Entities;
using MDMPI.App.Core.Logistic.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MDMPI.App.Data.Logistic.Repositories
{
    public class RequestAirSeaRepository(
        PostgreSqlAppDbContext db,
        IClientLookupRepository clientLookupRepository,
        ILogger<RequestAirSeaRepository> logger,
        IRequestIdGenerator requestIdGenerator
    ) : IRequestAirSeaRepository
    {
        private readonly PostgreSqlAppDbContext _db = db;
        private readonly IClientLookupRepository _clientLookupRepository = clientLookupRepository;
        private readonly ILogger<RequestAirSeaRepository> _logger = logger;
        private readonly IRequestIdGenerator _requestIdGenerator = requestIdGenerator;

        public async Task<List<RequestAirSeaDto>> GetAllAsync(RequestQueryDto query)
        {
            _logger.LogInformation("Fetching all air/sea requests.");

            var requests = _db.a_tblRequestAirSea.AsNoTracking();

            if (query.DateFilter != RequestDateFilter.All)
            {
                var today = DateTime.UtcNow.Date;
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
                        start = DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc);
                        end = DateTime.SpecifyKind(DateTime.MaxValue, DateTimeKind.Utc);
                        break;
                }
                requests = requests.Where(r => r.DatePickUp >= start && r.DatePickUp < end);
            }

            if (query.StatusFilter != RequestStatusFilter.All)
            {
                string? statusValue = query.StatusFilter switch
                {
                    RequestStatusFilter.NewRequest => "New Request",
                    RequestStatusFilter.GettingsSupliesReady => "Getting Supplies Ready",
                    RequestStatusFilter.ItemPrepared => "Item Prepared",
                    RequestStatusFilter.ItemPacked => "Item Packed",
                    RequestStatusFilter.EndorsedToGuard => "Endorsed To Guard",
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
                .Select(r => new RequestAirSeaDto
                {
                    RequestID = r.RequestID,
                    ItemCategoryID = r.ItemCategoryID,
                    ClientID = r.ClientID,
                    DocumentReference = r.DocumentReference != null
                        ? r.DocumentReference.Select(dr => dr.Reference).ToList()!
                        : new List<string>(),
                    MobileID = r.MobileID,
                    ReceivedBy = r.ReceivedBy,
                    WaybillNumber = r.WaybillNumber,
                    TripTicketNumber = r.TripTicketNumber,
                    Driver = r.Driver,
                    Helper = r.Helper,
                    DatePickUp = r.DatePickUp,
                    ItemPreparedAt = r.ItemPreparedAt,
                    ItemPreparedEndAt = r.ItemPreparedEndAt,
                    DispatchedAt = r.DispatchedAt,
                    DropOffAt = r.DropOffAt,
                    PreparedBy = r.PreparedBy,
                    Status = r.Status,
                    Remarks = r.Remarks,
                    CreatedBy = r.CreatedBy,
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt,
                    ProvincialPickUpBy = r.ProvincialPickUpBy,
                    ProvincialPickUpAt = r.ProvincialPickUpAt,
                    ProvincialInTransitAt = r.ProvincialInTransitAt,
                    ProvincialInTransitLocation = r.ProvincialInTransitLocation,
                    ProvincialDeliveredEndAt = r.ProvincialDeliveredEndAt,
                    ProvincialDeliveredLocation = r.ProvincialDeliveredLocation,
                    ProvincialReceiverName = r.ProvincialReceiverName,
                })
                .ToListAsync();

            await PopulateClientsAsync(result, item => item.ClientID, (item, client) => item.Client = client);

            _logger.LogInformation("Fetched {Count} air/sea requests.", result.Count);

            return result;
        }

        public async Task<RequestAirSeaDto?> InsertAsync(InsertRequestAirSeaDto dto)
        {
            using var transaction = await _db.Database.BeginTransactionAsync();

            try
            {
                _logger.LogInformation("Inserting new air/sea request for ClientID: {ClientID}", dto.ClientID);

                var newRequestIdLong = await _requestIdGenerator.GenerateAsync();

                var entity = new RequestAirSeaModel
                {
                    RequestID = newRequestIdLong,
                    ItemCategoryID = dto.ItemCategoryID,
                    ClientID = dto.ClientID,
                    CreatedBy = dto.CreatedBy,
                    DatePickUp = NormalizeToUtc(dto.DatePickUp),
                    Status = string.IsNullOrWhiteSpace(dto.Status) ? "New Request" : dto.Status,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedBy = dto.UpdatedBy,
                };

                _db.a_tblRequestAirSea.Add(entity);
                await _db.SaveChangesAsync();

                if (dto.DocumentReference is { Count: > 0 })
                {
                    var refs = dto.DocumentReference.Select(dr => new DocumentReferenceModel
                    {
                        RequestID = newRequestIdLong,
                        Reference = dr
                    });
                    _db.a_tblRequestDocumentReference.AddRange(refs);
                    await _db.SaveChangesAsync();
                }

                await transaction.CommitAsync();
                _logger.LogInformation("Inserted air/sea request with ID: {RequestID}", entity.RequestID);

                var inserted = await _db.a_tblRequestAirSea
                    .AsNoTracking()
                    .Where(r => r.RequestID == entity.RequestID)
                    .Select(r => new RequestAirSeaDto
                    {
                        RequestID = r.RequestID,
                        ItemCategoryID = r.ItemCategoryID,
                        ClientID = r.ClientID,
                        DocumentReference = _db.a_tblRequestDocumentReference
                            .Where(dr => dr.RequestID == r.RequestID)
                            .Select(dr => dr.Reference!)
                            .ToList(),
                        MobileID = r.MobileID,
                        ReceivedBy = r.ReceivedBy,
                        WaybillNumber = r.WaybillNumber,
                        TripTicketNumber = r.TripTicketNumber,
                        Driver = r.Driver,
                        Helper = r.Helper,
                        DatePickUp = r.DatePickUp,
                        ItemPreparedAt = r.ItemPreparedAt,
                        ItemPreparedEndAt = r.ItemPreparedEndAt,
                        DispatchedAt = r.DispatchedAt,
                        DropOffAt = r.DropOffAt,
                        PreparedBy = r.PreparedBy,
                        Status = r.Status,
                        Remarks = r.Remarks,
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
                await TransactionHelper.RollbackTransactionAsync(transaction, _logger, ex, "inserting Air/Sea request");
                return null;
            }
        }

        public async Task<bool> UpdateAsync(UpdateRequestAirSeaDto dto)
        {
            using var transaction = await _db.Database.BeginTransactionAsync();

            try
            {
                var entity = await _db.a_tblRequestAirSea.FirstOrDefaultAsync(x => x.RequestID == dto.RequestID);

                if (entity is null)
                {
                    return false;
                }

                QueryFilterHelper.UpdateIfNotNull(v => entity.Status = v, dto.Status);
                QueryFilterHelper.UpdateIfNotNull(v => entity.ReceivedBy = v, dto.ReceivedBy);
                QueryFilterHelper.UpdateIfNotNull(v => entity.WaybillNumber = v, dto.WaybillNumber);
                QueryFilterHelper.UpdateIfNotNull(v => entity.TripTicketNumber = v, dto.TripTicketNumber);
                QueryFilterHelper.UpdateIfNotNull(v => entity.Driver = v, dto.Driver);
                QueryFilterHelper.UpdateIfNotNull(v => entity.Helper = v, dto.Helper);
                QueryFilterHelper.UpdateIfNotNull(v => entity.ItemPreparedAt = NormalizeToUtc(v), dto.ItemPreparedAt);
                QueryFilterHelper.UpdateIfNotNull(v => entity.ItemPreparedEndAt = NormalizeToUtc(v), dto.ItemPreparedEndAt);
                QueryFilterHelper.UpdateIfNotNull(v => entity.DispatchedAt = NormalizeToUtc(v), dto.DispatchedAt);
                QueryFilterHelper.UpdateIfNotNull(v => entity.DropOffAt = NormalizeToUtc(v), dto.DropOffAt);
                QueryFilterHelper.UpdateIfNotNull(v => entity.PreparedBy = v, dto.PreparedBy);
                QueryFilterHelper.UpdateIfNotNull(v => entity.MobileID = v, dto.MobileID);
                QueryFilterHelper.UpdateIfNotNull(v => entity.Remarks = v, dto.Remarks);
                QueryFilterHelper.UpdateIfNotNull(v => entity.ProvincialPickUpBy = v, dto.ProvincialPickUpBy);
                QueryFilterHelper.UpdateIfNotNull(v => entity.ProvincialPickUpAt = NormalizeToUtc(v), dto.ProvincialPickUpAt);
                QueryFilterHelper.UpdateIfNotNull(v => entity.ProvincialInTransitAt = NormalizeToUtc(v), dto.ProvincialInTransitAt);
                QueryFilterHelper.UpdateIfNotNull(v => entity.ProvincialInTransitLocation = v, dto.ProvincialInTransitLocation);
                QueryFilterHelper.UpdateIfNotNull(v => entity.ProvincialReceiverName = v, dto.ProvincialReceiverName);
                QueryFilterHelper.UpdateIfNotNull(v => entity.ProvincialDeliveredEndAt = NormalizeToUtc(v), dto.ProvincialDeliveredEndAt);
                QueryFilterHelper.UpdateIfNotNull(v => entity.ProvincialDeliveredLocation = v, dto.ProvincialDeliveredLocation);
                QueryFilterHelper.UpdateIfNotNull(v => entity.UpdatedBy = v, dto.UpdatedBy);

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();
                _logger.LogInformation("Updated air/sea request with ID: {RequestID}", dto.RequestID);

                return true;
            }
            catch (Exception ex)
            {
                await TransactionHelper.RollbackTransactionAsync(transaction, _logger, ex, "updating Air/Sea request");
                return false;
            }
        }

        public async Task<List<RequestAirSeaHistoryDto>> GetHistoryAsync(long requestId)
        {
            var items = await _db.a_tblRequestAirSea_History
                .AsNoTracking()
                .Where(h => h.RequestID == requestId)
                .Select(h => new RequestAirSeaHistoryDto
                {
                    HistoryID = h.HistoryID,
                    ChangedAt = h.ChangedAt,
                    ActionType = h.ActionType,
                    RequestID = h.RequestID,
                    ClientID = h.ClientID,
                    ItemCategoryID = h.ItemCategoryID,
                    ReceivedBy = h.ReceivedBy,
                    WaybillNumber = h.WaybillNumber,
                    TripTicketNumber = h.TripTicketNumber,
                    Driver = h.Driver,
                    Helper = h.Helper,
                    MobileID = h.MobileID,
                    DatePickUp = h.DatePickUp,
                    ItemPreparedAt = h.ItemPreparedAt,
                    ItemPreparedEndAt = h.ItemPreparedEndAt,
                    DispatchedAt = h.DispatchedAt,
                    DropOffAt = h.DropOffAt,
                    PreparedBy = h.PreparedBy,
                    Status = h.Status,
                    Remarks = h.Remarks,
                    CreatedBy = h.CreatedBy,
                    CreatedAt = h.CreatedAt,
                    UpdatedAt = h.UpdatedAt,
                    ChangedBy = h.ChangedBy,
                })
                .ToListAsync();

            return items;
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

        private static DateTime? NormalizeToUtc(DateTime? value)
        {
            if (!value.HasValue)
            {
                return null;
            }

            return value.Value.Kind switch
            {
                DateTimeKind.Utc => value.Value,
                DateTimeKind.Local => value.Value.ToUniversalTime(),
                _ => DateTime.SpecifyKind(value.Value, DateTimeKind.Utc)
            };
        }
    }
}
