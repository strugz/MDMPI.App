using MDMPI.App.Core.Common.DTOs;
using MDMPI.App.Core.Common.Entities;
using MDMPI.App.Core.Common.Services;
using MDMPI.App.Core.CommonOldEntities.DTOs;
using MDMPI.App.Core.Logistic.DTOs;
using MDMPI.App.Core.Logistic.Entities;
using MDMPI.App.Core.Logistic.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MDMPI.App.Data.Logistic.Repositories
{
    public class RequestRepository : IRequestRepository
    {
        private readonly AppDbContext _db;
        private readonly ILogger<RequestRepository> _logger;

        public RequestRepository(AppDbContext db, ILogger<RequestRepository> logger)
        {
            _db = db;
            _logger = logger;
        }

        /// <summary>
        /// Gets all requests as DTOs.
        /// </summary>
        public async Task<List<RequestStandardDto>> GetAllRequestsAsync()
        {
            _logger.LogInformation("Fetching all requests.");
            var result = await _db.a_tblRequest
                .AsNoTracking()
                .Select(r => new RequestStandardDto
                {
                    ID = r.RequestID,
                    ClientID = r.RequestClientID,
                    ShippingMethod = r.RequestShippingMethod,
                    DeliveryTerms = r.RequestDeliveryTerms,
                    DeliveryDate = r.RequestDeliveryDate,
                    Preference = r.RequestPreference,
                    Status = r.RequestStatus,
                    RequestBy = r.RequestBy,
                    CreatedBy = r.RequestCreatedBy,
                    ItemPreparedBy = r.RequestItemPreparedBy,
                    DeliveredBy = r.RequestDeliveredBy,
                    ItemPreparedAt = r.RequestItemPreparedAt,
                    ItemPreparedEndAt = r.RequestItemPreparedEndAt,
                    DeliveredAt = r.RequestDeliveredAt,
                    DeliveredEndAt = r.RequestDeliveredEndAt,
                    MobileID = r.MobileID,
                    MobileName = r.Mobile!.MobileName,
                    Helper = r.RequestDriverHelper,
                    Receiver = r.Receiver,
                    TripTicketNumber = r.RequestTripTicketNumber,
                    DocumentReference = r.DocumentReference != null
                        ? r.DocumentReference.Select(dr => dr.Reference).ToList()
                        : new List<string>(),
                    Client = r.Client == null ? null : new ACCMSTDto
                    {
                        ACCMID = r.Client.ACCMID,
                        ACCMSC = r.Client.ACCMSC,
                        ACCMNM = r.Client.ACCMNM,
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
        /// Inserts a new request and returns true if successful.
        /// </summary>
        public async Task<bool> InsertRequest(InsertRequestDto dto)
        {
            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                _logger.LogInformation("Inserting new request for ClientID: {ClientID}", dto.RequestClientID);
                var request = new RequestStandardModel
                {
                    RequestClientID = dto.RequestClientID,
                    RequestShippingMethod = dto.RequestShippingMethod,
                    RequestDeliveryTerms = dto.RequestDeliveryTerms,
                    RequestDeliveryDate = dto.RequestDeliveryDate,
                    RequestPreference = dto.RequestPreference,
                    RequestStatus = dto.RequestStatus,
                    RequestBy = dto.RequestBy,
                    RequestCreatedBy = dto.RequestCreatedBy,
                    DocumentReference = dto.DocumentReference?.Select(dr => new DocumentReferenceModel
                    {
                        Reference = dr
                    }).ToList() ?? new List<DocumentReferenceModel>()
                };

                _db.a_tblRequest.Add(request);
                var result = await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                if (result > 0)
                {
                    _logger.LogInformation("Inserted request with ID: {RequestID}", request.RequestID);
                    return true;
                }
                else
                {
                    _logger.LogWarning("Insert failed for ClientID: {ClientID}", dto.RequestClientID);
                    return false;
                }
            }
            catch (Exception ex)
            {
                await Helper.RollbackTransactionAsync(transaction, _logger, ex, $"Error inserting request for ClientID: {dto.RequestClientID}");
                return false;
            }
        }

        /// <summary>
        /// Gets all remarks for a request.
        /// </summary>
        public async Task<RemarksDto?> GetAllRemarks(string requestid)
        {
            _logger.LogInformation("Fetching remarks for RequestID: {RequestID}", requestid);
            if (!long.TryParse(requestid, out var id))
            {
                _logger.LogWarning("Invalid RequestID format: {RequestID}", requestid);
                return null;
            }

            var remarks = await _db.a_tblRequestRemarks
                .AsNoTracking()
                .Where(r => r.RequestID == id)
                .Select(r => new RemarksDto
                {
                    RequestID = r.RequestID,
                    Remarks = r.Remarks,
                    Date = r.Date.HasValue
                        ? r.Date.Value.ToString("yyyy-MM-dd HH:mm:ss")
                        : null
                })
                .FirstOrDefaultAsync();

            _logger.LogInformation("Fetched remarks for RequestID: {RequestID}", requestid);
            return remarks;
        }

        /// <summary>
        /// Gets the proof image for a request.
        /// </summary>
        public async Task<byte[]?> GetRequestProofImage(string requestid)
        {
            _logger.LogInformation("Fetching proof image for RequestID: {RequestID}", requestid);
            if (!long.TryParse(requestid, out var id))
            {
                _logger.LogWarning("Invalid RequestID format: {RequestID}", requestid);
                return null;
            }

            var image = await _db.a_tblRequestImage
                .AsNoTracking()
                .Where(r => r.RequestID == id)
                .Select(r => r.RequestImage)
                .FirstOrDefaultAsync();

            _logger.LogInformation("Fetched proof image for RequestID: {RequestID}", requestid);
            return image;
        }

        /// <summary>
        /// Gets the signature image for a request.
        /// </summary>
        public async Task<byte[]?> GetRequestSignatureImage(string requestid)
        {
            _logger.LogInformation("Fetching signature image for RequestID: {RequestID}", requestid);
            if (!long.TryParse(requestid, out var id))
            {
                _logger.LogWarning("Invalid RequestID format: {RequestID}", requestid);
                return null;
            }

            var imageBase64 = await _db.a_tblRequestReceiverSignature
                .AsNoTracking()
                .Where(r => r.RequestID == id)
                .Select(r => r.RequestReceiverSignature)
                .FirstOrDefaultAsync();

            if (string.IsNullOrWhiteSpace(imageBase64))
            {
                _logger.LogWarning("No signature image found for RequestID: {RequestID}", requestid);
                return null;
            }

            try
            {
                var image = Convert.FromBase64String(imageBase64);
                _logger.LogInformation("Fetched signature image for RequestID: {RequestID}", requestid);
                return image;
            }
            catch (FormatException)
            {
                _logger.LogError("Invalid base64 string for signature image, RequestID: {RequestID}", requestid);
                return null;
            }
        }

        /// <summary>
        /// Updates an existing request with the provided data.
        /// </summary>
        public async Task<bool> UpdateRequest(UpdateRequestDto dto)
        {
            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                _logger.LogInformation("Updating request with ID: {RequestID}", dto.RequestID);

                var request = await _db.a_tblRequest
                    .FirstOrDefaultAsync(r => r.RequestID == dto.RequestID);

                if (request == null)
                {
                    _logger.LogWarning("Request not found for ID: {RequestID}", dto.RequestID);
                    return false;
                }

                // Conditionally update all fields using the generic helper
                Helper.UpdateIfNotNull(v => request.RequestStatus = v, dto.RequestStatus);
                Helper.UpdateIfNotNull(v => request.RequestItemPreparedBy = v, dto.RequestItemPreparedBy);
                Helper.UpdateIfNotNull(v => request.RequestDeliveredBy = v, dto.RequestDeliveredBy);
                Helper.UpdateIfNotNull(v => request.RequestDriverHelper = v, dto.RequestDriverHelper);
                Helper.UpdateIfNotNull(v => request.MobileID = v, dto.MobileID);
                Helper.UpdateIfNotNull(v => request.Receiver = v, dto.Receiver);
                Helper.UpdateIfNotNull(v => request.RequestTripTicketNumber = v, dto.RequestTripTicketNumber);
                Helper.UpdateIfNotNull(v => request.RequestItemPreparedAt = v, dto.RequestItemPreparedAt);
                Helper.UpdateIfNotNull(v => request.RequestItemPreparedEndAt = v, dto.RequestItemPreparedEndAt);
                Helper.UpdateIfNotNull(v => request.RequestDeliveredAt = v, dto.RequestDeliveredAt);
                Helper.UpdateIfNotNull(v => request.RequestDeliveredEndAt = v, dto.RequestDeliveredEndAt);
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

        /// <summary>
        /// Inserts a remark for a request and cancels the request atomically.
        /// </summary>
        public async Task<bool> InsertRemarkAndCancelRequestAsync(long requestId, string remarks)
        {
            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                // Insert remark
                var remark = new RemarksModel
                {
                    RequestID = requestId,
                    Remarks = remarks,
                    Date = DateTime.UtcNow
                };
                _db.a_tblRequestRemarks.Add(remark);

                // Update request status
                var request = await _db.a_tblRequest.FirstOrDefaultAsync(r => r.RequestID == requestId);
                if (request == null)
                {
                    _logger.LogWarning("Request not found for ID: {RequestID}", requestId);
                    return false;
                }
                request.RequestStatus = "Cancelled";

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Inserted remark and cancelled request with ID: {RequestID}", requestId);
                return true;
            }
            catch (Exception ex)
            {
                await Helper.RollbackTransactionAsync(transaction, _logger, ex, $"Error inserting remark and cancelling request for ID: {requestId}");
                return false;
            }
        }
    }
}
