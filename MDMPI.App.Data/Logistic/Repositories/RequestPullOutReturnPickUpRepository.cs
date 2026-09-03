using MDMPI.App.Core.Common.DTOs;
using MDMPI.App.Core.Common.Entities;
using MDMPI.App.Core.Common.Interfaces;
using MDMPI.App.Core.Common.Services;
using MDMPI.App.Core.Logistic.DTOs.RequestPullOutReturnPickUp;
using MDMPI.App.Core.Logistic.Entities;
using MDMPI.App.Core.Logistic.Interfaces;
using MDMPI.App.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MDMPI.App.Data.Logistic.Repositories
{
    public class RequestPullOutReturnPickUpRepository : IRequestPullOutReturnPickUpRepository
    {
        private readonly PostgreSqlAppDbContext _db;
        private readonly IClientLookupRepository _clientLookupRepository;
        private readonly ILogger<RequestPullOutReturnPickUpRepository> _logger;
        private readonly IRequestIdGenerator _requestIdGenerator;

        public RequestPullOutReturnPickUpRepository(
            PostgreSqlAppDbContext db,
            IClientLookupRepository clientLookupRepository,
            ILogger<RequestPullOutReturnPickUpRepository> logger,
            IRequestIdGenerator requestIdGenerator)
        {
            _db = db;
            _clientLookupRepository = clientLookupRepository;
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
            requests = QueryFilterHelper.ApplyDateFilterAny(
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
                    RequestStatusFilter.ForPullOut => "For Pull Out",
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
                })
                .ToListAsync();

            await PopulateClientsAsync(result, item => item.ClientID, (item, client) => item.Client = client);

            _logger.LogInformation("Fetched {Count} pull-out/return/pick-up requests.", result.Count);
            return result;
        }

        /// <summary>
        /// Inserts a new pull-out/return/pick-up request into the database, including document references if provided, and returns the inserted DTO.
        /// </summary>
        public async Task<RequestPullOutReturnPickUpDto?> InsertAsync(InsertRequestPullOutReturnPickUpDto dto)
        {
            using var transaction = await _db.Database.BeginTransactionAsync();

            try
            {
                _logger.LogInformation("Inserting new request for ClientID: {ClientID}", dto.ClientID);

                var newRequestId = await _requestIdGenerator.GenerateAsync();

                var request = QueryFilterHelper.BuilbRequestPullOutReturnPickUpModel(dto, newRequestId);
                NormalizeRequestDateTimes(request);

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

                var inserted = await _db.a_tblRequestPullOutReturnPickUp
                    .AsNoTracking()
                    .Where(r => r.RequestID == request.RequestID)
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
                        DocumentReference = _db.a_tblRequestDocumentReference
                            .Where(dr => dr.RequestID == r.RequestID)
                            .Select(dr => dr.Reference!)
                            .ToList(),
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
                await TransactionHelper.RollbackTransactionAsync(transaction, _logger, ex, "inserting PullOut/Return/PickUp");
                return null;
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

                QueryFilterHelper.UpdateIfNotNull(v => request.ClientContactPerson = v, dto.ClientContactPerson);
                QueryFilterHelper.UpdateIfNotNull(v => request.ReasonForReturn = v, dto.ReasonForReturn);
                QueryFilterHelper.UpdateIfNotNull(v => request.ReleasedBy = v, dto.ReleasedBy);
                if (dto.ClearPullOutDateStartAt == true)
                {
                    // Pause: courier returns the request to For Pull Out; the
                    // next departure stamps a fresh start time.
                    request.PullOutDateStartAt = null;
                }
                else if (dto.PullOutDateStartAt.HasValue)
                {
                    request.PullOutDateStartAt = NormalizeToUtc(dto.PullOutDateStartAt.Value);
                }

                if (dto.PullOutDateEndAt.HasValue)
                {
                    request.PullOutDateEndAt = NormalizeToUtc(dto.PullOutDateEndAt.Value);
                }
                QueryFilterHelper.UpdateIfNotNull(v => request.MobileID = v, dto.MobileID);
                QueryFilterHelper.UpdateIfNotNull(v => request.RequestStatus = v, dto.RequestStatus);
                QueryFilterHelper.UpdateIfNotNull(v => request.TripTicketNumber = v, dto.TripTicketNumber);
                QueryFilterHelper.UpdateIfNotNull(v => request.Driver = v, dto.Driver);
                QueryFilterHelper.UpdateIfNotNull(v => request.Helper = v, dto.Helper);
                QueryFilterHelper.UpdateIfNotNull(v => request.ReceivedBy = v, dto.ReceivedBy);
                request.UpdatedAt = DateTime.UtcNow;

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Updated pull-out/return/pick-up request with ID: {RequestID}", dto.RequestID);

                return true;
            }
            catch (Exception ex)
            {
                await TransactionHelper.RollbackTransactionAsync(transaction, _logger, ex, "updating PullOut/Return/PickUp");
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

        private static void NormalizeRequestDateTimes(RequestPullOutReturnPickUpModel request)
        {
            request.PullOutDateStartAt = NormalizeNullableUtc(request.PullOutDateStartAt);
            request.PullOutDateEndAt = NormalizeNullableUtc(request.PullOutDateEndAt);
            request.CreatedAt = NormalizeNullableUtc(request.CreatedAt) ?? DateTime.UtcNow;
            request.UpdatedAt = NormalizeNullableUtc(request.UpdatedAt);
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
