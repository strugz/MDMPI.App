using MDMPI.App.Common.Utilities;
using MDMPI.App.Core.Common.DTOs;
using MDMPI.App.Core.Common.Entities;
using MDMPI.App.Core.Common.Interfaces;
using MDMPI.App.Core.Common.Services;
using MDMPI.App.Data.Common;
using MDMPI.App.Core.Logistic.DTOs.RequestStandard;
using MDMPI.App.Core.Logistic.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MDMPI.App.Data.Logistic.Repositories
{
    public class RequestRepository : IRequestRepository
    {
        private readonly AppDbContext _db;
        private readonly ILogger<RequestRepository> _logger;
        private readonly IRequestIdGenerator _requestIdGenerator;

        public RequestRepository(
            AppDbContext db,
            ILogger<RequestRepository> logger,
            IRequestIdGenerator requestIdGenerator,
            IItemIdGenerator itemIdGenerator,
            IBatchIdGenerator batchIdGenerator)
        {
            _db = db;
            _logger = logger;
            _requestIdGenerator = requestIdGenerator;

        }

        /// <summary>
        /// Gets history records for a specific request.
        /// </summary>
        public async Task<List<RequestStandardHistoryDto>> GetAllRequestHistory(long requestId)
        {
            if (requestId <= 0)
            {
                _logger.LogWarning("Request history lookup skipped due to invalid RequestID: {RequestID}", requestId);
                return new List<RequestStandardHistoryDto>();
            }

            _logger.LogInformation("Fetching history for RequestID: {RequestID}", requestId);

            var history = await _db.a_tblRequestStandardDeliveryHistory
                .AsNoTracking()
                .Where(h => h.RequestID == requestId)
                .OrderByDescending(h => h.ChangedAt)
                .ThenByDescending(h => h.HistoryID)
                .Select(h => new RequestStandardHistoryDto
                {
                    HistoryID = h.HistoryID,
                    ActionType = h.ActionType,
                    ChangedAt = h.ChangedAt,
                    ChangedBy = h.ChangedBy,
                    RequestID = h.RequestID,
                    ItemCategoryID = h.ItemCategoryID,
                    FormCategoryID = h.FormCategoryID,
                    RequestClientID = h.RequestClientID,
                    RequestShippingMethod = h.RequestShippingMethod,
                    RequestDeliveryTerms = h.RequestDeliveryTerms,
                    RequestDeliveryDate = h.RequestDeliveryDate,
                    RequestPreference = h.RequestPreference,
                    RequestStatus = h.RequestStatus,
                    RequestBy = h.RequestBy,
                    RequestCreatedBy = h.RequestCreatedBy,
                    RequestItemPreparedBy = h.RequestItemPreparedBy,
                    RequestDeliveredBy = h.RequestDeliveredBy,
                    RequestCreatedAt = h.RequestCreatedAt,
                    RequestItemPreparedAt = h.RequestItemPreparedAt,
                    RequestItemPreparedEndAt = h.RequestItemPreparedEndAt,
                    RequestDeliveredAt = h.RequestDeliveredAt,
                    RequestDeliveredEndAt = h.RequestDeliveredEndAt,
                    LocationStartedAt = h.LocationStartedAt,
                    LocationEndAt = h.LocationEndAt,
                    MobileID = h.MobileID,
                    RequestDriverHelper = h.RequestDriverHelper,
                    Receiver = h.Receiver,
                    RequestTripTicketNumber = h.RequestTripTicketNumber
                })
                .ToListAsync();

            _logger.LogInformation("Fetched {Count} history records for RequestID: {RequestID}", history.Count, requestId);
            return history;
        }

        /// <summary>
        /// Gets all requests as DTOs with paging, sorting, and date filtering.
        /// </summary>
        public async Task<List<RequestStandardDto>> GetAllRequestsAsync(RequestQueryDto query)
        {
            _logger.LogInformation("Fetching requests with filters: {@Query}", query);
            var requests = _db.a_tblRequestStandardDelivery.AsNoTracking();

            requests = QueryFilterHelper.ApplyDateFilterAny(
                requests,
                query.DateFilter,
                r => r.RequestDeliveryDate.HasValue ? r.RequestDeliveryDate.Value : null
            );

            // Apply status filter from RequestQueryDto.StatusFilter
            if (query.StatusFilter != RequestStatusFilter.All)
            {
                // Map enum value to the string stored in the RequestStatus column.
                // Adjust the mapped strings if your DB stores different text.
                string? statusValue = query.StatusFilter switch
                {
                    RequestStatusFilter.NewRequest => "New Request",
                    RequestStatusFilter.GettingsSupliesReady => "Getting Supplies Ready",
                    RequestStatusFilter.ItemPrepared => "Item Prepared",
                    RequestStatusFilter.ForDelivery => "For Delivery",
                    RequestStatusFilter.InTransit => "In Transit",
                    RequestStatusFilter.Delivered => "Delivered",
                    RequestStatusFilter.Cancelled => "Cancelled",
                    _ => null
                };

                if (!string.IsNullOrWhiteSpace(statusValue))
                {
                    requests = requests.Where(r => r.RequestStatus == statusValue);
                }
            }

            var result = await requests
                .Select(r => new RequestStandardDto
                {
                    ID = r.RequestID.ToString(),
                    ClientID = r.RequestClientID,
                    FormCategoryID = r.FormCategoryID,
                    ItemCategoryID = r.ItemCategoryID,
                    ShippingMethod = r.RequestShippingMethod,
                    DeliveryTerms = r.RequestDeliveryTerms,
                    DeliveryDate = r.RequestDeliveryDate.HasValue ? r.RequestDeliveryDate.Value.ToString("yyyy-MM-dd") : null,
                    Preference = r.RequestPreference,
                    Status = r.RequestStatus,
                    RequestBy = r.RequestBy,
                    CreatedBy = r.RequestCreatedBy,
                    CreatedAt = r.RequestCreatedAt.HasValue ? r.RequestCreatedAt.Value.ToString("yyyy-MM-dd HH:mm:ss") : null,
                    ItemPreparedBy = r.RequestItemPreparedBy,
                    DeliveredBy = r.RequestDeliveredBy,
                    ItemPreparedAt = r.RequestItemPreparedAt.HasValue ? r.RequestItemPreparedAt.Value.ToString("yyyy-MM-dd HH:mm:ss") : null,
                    ItemPreparedEndAt = r.RequestItemPreparedEndAt.HasValue ? r.RequestItemPreparedEndAt.Value.ToString("yyyy-MM-dd HH:mm:ss") : null,
                    DeliveredAt = r.RequestDeliveredAt.HasValue ? r.RequestDeliveredAt.Value.ToString("yyyy-MM-dd HH:mm:ss") : null,
                    DeliveredEndAt = r.RequestDeliveredEndAt.HasValue ? r.RequestDeliveredEndAt.Value.ToString("yyyy-MM-dd HH:mm:ss") : null,
                    MobileID = r.MobileID,
                    MobileName = r.Mobile != null ? r.Mobile.MobileName : null,
                    Helper = r.RequestDriverHelper,
                    Receiver = r.Receiver,
                    RecipientName = r.RecipientName,
                    RecipientContactDetails = r.RecipientContactDetails,
                    TripTicketNumber = r.RequestTripTicketNumber,
                    DocumentReference = r.DocumentReference != null
                        ? r.DocumentReference.Select(dr => dr.Reference).ToList()!
                        : new List<string>(),
                    Client = r.Client == null ? null : new ACCMSTDto
                    {
                        ACCMID = r.Client.ACCMID,
                        ACCMSC = r.Client.ACCMSC,
                        ACCMNM = r.Client.ACCMNM.ToProperCase(),
                        ACCMBC = r.Client.ACCMBC,
                        ACCMAD = r.Client.ACCMAD,
                        ACCMPH = r.Client.ACCMPH,
                        ACCMEM = r.Client.ACCMEM,
                        ACCMWS = r.Client.ACCMWS,
                        ACCSTS = r.Client.ACCSTS,
                        ACCOWN = r.Client.ACCOWN
                    }
                })
                .ToListAsync();

            _logger.LogInformation("Fetched {Count} requests.", result.Count);
            return result;
        }

        /// <summary>
        /// Inserts a new request and returns the inserted DTO if successful.
        /// </summary>
        public async Task<RequestStandardDto?> InsertRequest(InsertRequestDto dto)
        {
            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                _logger.LogInformation("Inserting new request for ClientID: {ClientID}", dto.RequestClientID);

                var newRequestId = await _requestIdGenerator.GenerateAsync();
                var request = QueryFilterHelper.BuildRequestStandardModel(dto, newRequestId);

                _db.a_tblRequestStandardDelivery.Add(request);
                await _db.SaveChangesAsync();

                if (dto.DocumentReference is not null && dto.DocumentReference.Count > 0)
                {
                    var refs = dto.DocumentReference.Select(dr => new DocumentReferenceModel
                    {
                        RequestID = request.RequestID,
                        Reference = dr
                    });
                    _db.a_tblRequestDocumentReference.AddRange(refs);
                    await _db.SaveChangesAsync();
                }

                await transaction.CommitAsync();
                _logger.LogInformation("Inserted request with ID: {RequestID}", request.RequestID);

                var inserted = await _db.a_tblRequestStandardDelivery
                    .AsNoTracking()
                    .Where(r => r.RequestID == request.RequestID)
                    .Select(r => new RequestStandardDto
                    {
                        ID = r.RequestID.ToString(),
                        ClientID = r.RequestClientID,
                        FormCategoryID = r.FormCategoryID,
                        ItemCategoryID = r.ItemCategoryID,
                        ShippingMethod = r.RequestShippingMethod,
                        DeliveryTerms = r.RequestDeliveryTerms,
                        DeliveryDate = r.RequestDeliveryDate.HasValue ? r.RequestDeliveryDate.Value.ToString("yyyy-MM-dd") : null,
                        Preference = r.RequestPreference,
                        Status = r.RequestStatus,
                        RequestBy = r.RequestBy,
                        CreatedBy = r.RequestCreatedBy,
                        CreatedAt = r.RequestCreatedAt.HasValue ? r.RequestCreatedAt.Value.ToString("yyyy-MM-dd HH:mm:ss") : null,
                        ItemPreparedBy = r.RequestItemPreparedBy,
                        DeliveredBy = r.RequestDeliveredBy,
                        ItemPreparedAt = r.RequestItemPreparedAt.HasValue ? r.RequestItemPreparedAt.Value.ToString("yyyy-MM-dd HH:mm:ss") : null,
                        ItemPreparedEndAt = r.RequestItemPreparedEndAt.HasValue ? r.RequestItemPreparedEndAt.Value.ToString("yyyy-MM-dd HH:mm:ss") : null,
                        DeliveredAt = r.RequestDeliveredAt.HasValue ? r.RequestDeliveredAt.Value.ToString("yyyy-MM-dd HH:mm:ss") : null,
                        DeliveredEndAt = r.RequestDeliveredEndAt.HasValue ? r.RequestDeliveredEndAt.Value.ToString("yyyy-MM-dd HH:mm:ss") : null,
                        MobileID = r.MobileID,
                        MobileName = r.Mobile != null ? r.Mobile.MobileName : null,
                        Helper = r.RequestDriverHelper,
                        Receiver = r.Receiver,
                        RecipientName = r.RecipientName,
                        RecipientContactDetails = r.RecipientContactDetails,
                        TripTicketNumber = r.RequestTripTicketNumber,
                        DocumentReference = _db.a_tblRequestDocumentReference
                            .Where(dr => dr.RequestID == r.RequestID)
                            .Select(dr => dr.Reference!)
                            .ToList(),
                        Client = r.Client == null ? null : new ACCMSTDto
                        {
                            ACCMID = r.Client.ACCMID,
                            ACCMSC = r.Client.ACCMSC,
                            ACCMNM = r.Client.ACCMNM.ToProperCase(),
                            ACCMBC = r.Client.ACCMBC,
                            ACCMAD = r.Client.ACCMAD,
                            ACCMPH = r.Client.ACCMPH,
                            ACCMEM = r.Client.ACCMEM,
                            ACCMWS = r.Client.ACCMWS,
                            ACCSTS = r.Client.ACCSTS,
                            ACCOWN = r.Client.ACCOWN
                        },
                        UpdatedBy = r.UpdatedBy
                    })
                    .FirstOrDefaultAsync();

                return inserted;
            }
            catch (Exception ex)
            {
                await TransactionHelper.RollbackTransactionAsync(transaction, _logger, ex, $"Error inserting request for ClientID: {dto.RequestClientID}");
                return null;
            }
        }

        /// <summary>
        /// Updates an existing request with the provided data (including Image, Signature, and Remarks upserts).
        /// </summary>
        public async Task<bool> UpdateRequest(UpdateRequestDto dto)
        {
            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                _logger.LogInformation("Updating request with ID: {RequestID}", dto.RequestID);

                var request = await _db.a_tblRequestStandardDelivery
                    .FirstOrDefaultAsync(r => r.RequestID == long.Parse(dto.RequestID!));

                if (request == null)
                {
                    _logger.LogWarning("Request not found for ID: {RequestID}", dto.RequestID);
                    return false;
                }

                // Conditionally update scalar fields
                QueryFilterHelper.UpdateIfNotNull(v => request.RequestStatus = v, dto.RequestStatus);
                QueryFilterHelper.UpdateIfNotNull(v => request.RequestItemPreparedBy = v, dto.RequestItemPreparedBy);
                QueryFilterHelper.UpdateIfNotNull(v => request.RequestDeliveredBy = v, dto.RequestDeliveredBy);
                QueryFilterHelper.UpdateIfNotNull(v => request.RequestDriverHelper = v, dto.RequestDriverHelper);
                QueryFilterHelper.UpdateIfNotNull(v => request.MobileID = v, dto.MobileID);
                QueryFilterHelper.UpdateIfNotNull(v => request.Receiver = v, dto.Receiver);
                QueryFilterHelper.UpdateIfNotNull(v => request.RequestTripTicketNumber = v, dto.RequestTripTicketNumber);
                QueryFilterHelper.UpdateIfNotNull(v => request.RequestItemPreparedAt = DateTime.TryParse(v, out var dt) ? dt : (DateTime?)null, dto.RequestItemPreparedAt);
                QueryFilterHelper.UpdateIfNotNull(v => request.RequestItemPreparedEndAt = DateTime.TryParse(v, out var dt) ? dt : (DateTime?)null, dto.RequestItemPreparedEndAt);
                QueryFilterHelper.UpdateIfNotNull(v => request.RequestDeliveredAt = DateTime.TryParse(v, out var dt) ? dt : (DateTime?)null, dto.RequestDeliveredAt);
                QueryFilterHelper.UpdateIfNotNull(v => request.RequestDeliveredEndAt = DateTime.TryParse(v, out var dt) ? dt : (DateTime?)null, dto.RequestDeliveredEndAt);
                QueryFilterHelper.UpdateIfNotNull(v => request.LocationStartedAt = v, dto.LocationStartedAt);
                QueryFilterHelper.UpdateIfNotNull(v => request.LocationEndAt = v, dto.LocationEndAt);
                QueryFilterHelper.UpdateIfNotNull(v => request.UpdatedBy = v, dto.UpdatedBy);

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();
                _logger.LogInformation("Updated request with ID: {RequestID}", dto.RequestID);

                return true;
            }
            catch (Exception ex)
            {
                await TransactionHelper.RollbackTransactionAsync(transaction, _logger, ex, $"Error updating request with ID: {dto.RequestID}");
                return false;
            }
        }


    }
}
