using MDMPI.App.Common.Utilities;
using MDMPI.App.Core.Common.DTOs;
using MDMPI.App.Core.Common.Entities;
using MDMPI.App.Core.Common.Interfaces;
using MDMPI.App.Core.Common.Services;
using MDMPI.App.Core.CommonOldEntities.DTOs;
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

        public RequestRepository(AppDbContext db, ILogger<RequestRepository> logger, IRequestIdGenerator requestIdGenerator)
        {
            _db = db;
            _logger = logger;
            _requestIdGenerator = requestIdGenerator;
        }

        /// <summary>
        /// Gets all requests as DTOs with paging, sorting, and date filtering.
        /// </summary>
        public async Task<List<RequestStandardDto>> GetAllRequestsAsync(RequestQueryDto query)
        {
            _logger.LogInformation("Fetching requests with filters: {@Query}", query);
            var requests = _db.a_tblRequestStandardDelivery.AsNoTracking();

            requests = Helper.ApplyDateFilterAny(
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

                var request = Helper.BuildRequestStandardModel(dto, newRequestId);

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
                        }
                    })
                    .FirstOrDefaultAsync();

                return inserted;
            }
            catch (Exception ex)
            {
                await Helper.RollbackTransactionAsync(transaction, _logger, ex, $"Error inserting request for ClientID: {dto.RequestClientID}");
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
                Helper.UpdateIfNotNull(v => request.RequestStatus = v, dto.RequestStatus);
                Helper.UpdateIfNotNull(v => request.RequestItemPreparedBy = v, dto.RequestItemPreparedBy);
                Helper.UpdateIfNotNull(v => request.RequestDeliveredBy = v, dto.RequestDeliveredBy);
                Helper.UpdateIfNotNull(v => request.RequestDriverHelper = v, dto.RequestDriverHelper);
                Helper.UpdateIfNotNull(v => request.MobileID = v, dto.MobileID);
                Helper.UpdateIfNotNull(v => request.Receiver = v, dto.Receiver);
                Helper.UpdateIfNotNull(v => request.RequestTripTicketNumber = v, dto.RequestTripTicketNumber);
                Helper.UpdateIfNotNull(v => request.RequestItemPreparedAt = DateTime.TryParse(v, out var dt) ? dt : (DateTime?)null, dto.RequestItemPreparedAt);
                Helper.UpdateIfNotNull(v => request.RequestItemPreparedEndAt = DateTime.TryParse(v, out var dt) ? dt : (DateTime?)null, dto.RequestItemPreparedEndAt);
                Helper.UpdateIfNotNull(v => request.RequestDeliveredAt = DateTime.TryParse(v, out var dt) ? dt : (DateTime?)null, dto.RequestDeliveredAt);
                Helper.UpdateIfNotNull(v => request.RequestDeliveredEndAt = DateTime.TryParse(v, out var dt) ? dt : (DateTime?)null, dto.RequestDeliveredEndAt);
                Helper.UpdateIfNotNull(v => request.LocationStartedAt = v, dto.LocationStartedAt);
                Helper.UpdateIfNotNull(v => request.LocationEndAt = v, dto.LocationEndAt);

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();
                _logger.LogInformation("Updated request with ID: {RequestID}", dto.RequestID);

                return true;
            }
            catch (Exception ex)
            {
                await Helper.RollbackTransactionAsync(transaction, _logger, ex, $"Error updating request with ID: {dto.RequestID}");
                return false;
            }
        }
    }
}
