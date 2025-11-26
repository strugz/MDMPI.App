using MDMPI.App.Common.Utilities;
using MDMPI.App.Core.Common.DTOs;
using MDMPI.App.Core.Common.Entities;
using MDMPI.App.Core.Common.Services;
using MDMPI.App.Core.CommonOldEntities.DTOs;
using MDMPI.App.Core.Logistic.DTOs.RequestPullOutReturnPickUp;
using MDMPI.App.Core.Logistic.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MDMPI.App.Data.Logistic.Repositories
{
    public class RequestPullOutReturnPickUpRepository : IRequestPullOutReturnPickUpRepository
    {
        private readonly AppDbContext _db;
        private readonly ILogger<RequestPullOutReturnPickUpRepository> _logger;
        private readonly IRequestIdGenerator _requestIdGenerator;

        public RequestPullOutReturnPickUpRepository(AppDbContext db, ILogger<RequestPullOutReturnPickUpRepository> logger, IRequestIdGenerator requestIdGenerator)
        {
            _db = db;
            _logger = logger;
            _requestIdGenerator = requestIdGenerator;
        }

        /// <summary>
        /// Retrieves all pull-out/return/pick-up requests from the database, applies date filtering, and returns a list of display DTOs.
        /// </summary>
        public async Task<List<RequestPullOutReturnPickUpDto>> GetAllAsync(RequestQueryDto query)
        {
            _logger.LogInformation("Fetching all pull-out/return/pick-up requests.");
            var requests = _db.a_tblRequestPullOutReturnPickUp.AsNoTracking();

            // PullOutDate is DateOnly? on the model, pass it directly to the helper
            requests = Helper.ApplyDateFilterAny(
                requests,
                query.DateFilter,
                r => r.PullOutDate
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
                .Select(r => new RequestPullOutReturnPickUpDto
                {
                    RequestID = r.RequestID,
                    ClientID = r.ClientID,
                    ClientContactPerson = r.ClientContactPerson,
                    FormCategoryID = r.FormCategoryID,
                    SlipNo = r.SlipNo,
                    IRRFNumber = r.IRRFNumber,
                    IRRFDate = r.IRRFDate,
                    ReasonForReturn = r.ReasonForReturn,
                    ReleasedBy = r.ReleasedBy,
                    ItemCategoryID = r.ItemCategoryID,
                    PullOutDate = r.PullOutDate,
                    PullOutDateEndAt = r.PullOutDateEndAt,
                    PullOutDateStartAt = r.PullOutDateStartAt,
                    RequestStatus = r.RequestStatus,
                    TripTicketNumber = r.TripTicketNumber,
                    Driver = r.Driver,
                    Helper = r.Helper,
                    ReceivedBy = r.ReceivedBy,
                    MobileID = r.MobileID,
                    MobileName = r.Mobile != null ? r.Mobile.MobileName : null,
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt,
                    CreatedBy = r.CreatedBy,
                    RequestedBy = r.RequestedBy,
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
            _logger.LogInformation("Fetched {Count} pull-out/return/pick-up requests.", result.Count);
            return result;
        }

        /// <summary>
        /// Inserts a new pull-out/return/pick-up request into the database, including document references if provided.
        /// </summary>
        public async Task<bool> InsertAsync(InsertRequestPullOutReturnPickUpDto dto)
        {
            using var transaction = await _db.Database.BeginTransactionAsync();

            try
            {
                _logger.LogInformation("Inserting new request for ClientID: {ClientID}", dto.ClientID);

                var newRequestId = await _requestIdGenerator.GenerateAsync();

                var request = Helper.BuilbRequestPullOutReturnPickUpModel(dto, newRequestId);

                _db.a_tblRequestPullOutReturnPickUp.Add(request);
                await _db.SaveChangesAsync();

                if (dto.DocumentReference is not null && dto.DocumentReference.Count > 0)
                {
                    var refs = dto.DocumentReference.Select(r => new DocumentReferenceModel
                    {
                        RequestID = request.RequestID,
                        Reference = r
                    });
                    _db.a_tblRequestDocumentReference.AddRange(refs);
                    await _db.SaveChangesAsync();
                }

                await transaction.CommitAsync();
                _logger.LogInformation("Inserted new request with ID: {RequestID}", request.RequestID);
                return true;
            }

            catch (Exception ex)
            {
                await Helper.RollbackTransactionAsync(transaction, _logger, ex, "inserting PullOut/Return/PickUp");
                return false;
            }
        }

        /// <summary>
        /// Updates an existing pull-out/return/pick-up request in the database, including remarks if provided.
        /// </summary>
        public async Task<bool> UpdateAsync(UpdateRequestPullOutReturnPickUpDto dto)
        {
            using var transaction = await _db.Database.BeginTransactionAsync();


            try
            {
                _logger.LogInformation("Updating pull-out/return/pick-up request with ID: {RequestID}", dto.RequestID);

                var request = await _db.a_tblRequestPullOutReturnPickUp
                    .FirstOrDefaultAsync(x => x.RequestID == dto.RequestID);

                if (request == null)
                {
                    _logger.LogWarning("Request with ID: {RequestID} not found.", dto.RequestID);
                    return false;
                }

                Helper.UpdateIfNotNull(v => request.ClientContactPerson = v, dto.ClientContactPerson);
                Helper.UpdateIfNotNull(v => request.ReasonForReturn = v, dto.ReasonForReturn);
                Helper.UpdateIfNotNull(v => request.ReleasedBy = v, dto.ReleasedBy);
                Helper.UpdateIfNotNull(v => request.PullOutDateStartAt = v, dto.PullOutDateStartAt);
                Helper.UpdateIfNotNull(v => request.PullOutDateEndAt = v, dto.PullOutDateEndAt);
                Helper.UpdateIfNotNull(v => request.MobileID = v, dto.MobileID);
                Helper.UpdateIfNotNull(v => request.RequestStatus = v, dto.RequestStatus);
                Helper.UpdateIfNotNull(v => request.TripTicketNumber = v, dto.TripTicketNumber);
                Helper.UpdateIfNotNull(v => request.Driver = v, dto.Driver);
                Helper.UpdateIfNotNull(v => request.Helper = v, dto.Helper);
                Helper.UpdateIfNotNull(v => request.ReceivedBy = v, dto.ReceivedBy);
                request.UpdatedAt = DateTime.UtcNow;
                var requestId = request.RequestID;

                var result = await _db.SaveChangesAsync();

                await transaction.CommitAsync();

                _logger.LogInformation("Updated pull-out/return/pick-up request with ID: {RequestID}", dto.RequestID);

                return true;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating pull-out/return/pick-up request with ID: {RequestID}", dto.RequestID);
                return false;
            }
        }
    }
}