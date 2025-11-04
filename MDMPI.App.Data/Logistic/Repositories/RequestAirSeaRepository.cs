using Azure.Core;
using MDMPI.App.Core.Common.DTOs;
using MDMPI.App.Core.Common.Entities;
using MDMPI.App.Core.Common.Services;
using MDMPI.App.Core.CommonOldEntities.DTOs;
using MDMPI.App.Core.Logistic.DTOs.RequestAirSea;
using MDMPI.App.Core.Logistic.Entities;
using MDMPI.App.Core.Logistic.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MDMPI.App.Data.Logistic.Repositories
{
    public class RequestAirSeaRepository : IRequestAirSeaRepository
    {
        private readonly AppDbContext _db;
        private readonly ILogger<RequestAirSeaRepository> _logger;
        private readonly IRequestIdGenerator _requestIdGenerator;

        public RequestAirSeaRepository(AppDbContext db, ILogger<RequestAirSeaRepository> logger, IRequestIdGenerator requestIdGenerator)
        {
            _db = db;
            _logger = logger;
            _requestIdGenerator = requestIdGenerator;
        }

        public async Task<List<RequestAirSeaDto>> GetAllAsync(RequestQueryDto query)
        {
            _logger.LogInformation("Fetching all air/sea requests.");

            var requests = _db.a_tblRequestAirSea.AsNoTracking();

            // Apply date filter on DatePickUp (translateable to SQL)
            if (query.DateFilter != RequestDateFilter.All)
            {
                var today = DateTime.Today;
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

            if (query.StatusFilter != RequestStatusFilter.All)
            {
                string? statusValue = query.StatusFilter switch
                {
                    RequestStatusFilter.NewRequest => "New Request",
                    RequestStatusFilter.GettingsSupliesReady => "Getting Supplies Ready",
                    RequestStatusFilter.ItemPrepared => "Item Prepared",
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
                .Select(r => new RequestAirSeaDto
                {
                    RequestID = r.RequestID,
                    ItemCategoryID = r.ItemCategoryID,
                    ClientID = r.ClientID,
                    DocumentReference = r.DocumentReference != null 
                    ? r.DocumentReference.Select(dr => dr.Reference).ToList()! 
                    : new List<string>(),
                    MobileID = r.MobileID,
                    RiderName = r.RiderName,
                    DatePickUp = r.DatePickUp,
                    ItemPreparedAt = r.ItemPreparedAt,
                    ItemPreparedEndAt = r.ItemPreparedEndAt,
                    PreparedBy = r.PreparedBy,
                    Status = r.Status,
                    Remarks = r.Remarks,
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt,
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

            _logger.LogInformation("Fetched {Count} air/sea requests.", result.Count);

            return result;
        }

        public async Task<bool> InsertAsync(InsertRequestAirSeaDto dto)
        {
            using var transaction = await _db.Database.BeginTransactionAsync();

            try
            {
                _logger.LogInformation("Inserting new air/sea request for ClientID: {ClientID}", dto.ClientID);

                var newRequestIdLong = await _requestIdGenerator.GenerateAsync();

                // Only populate the fields you specified: ItemCategoryID, ClientID, DatePickUp, Status
                var entity = new RequestAirSeaModel
                {
                    RequestID = newRequestIdLong,
                    ItemCategoryID = dto.ItemCategoryID,
                    ClientID = dto.ClientID,
                    DatePickUp = dto.DatePickUp,
                    Status = string.IsNullOrWhiteSpace(dto.Status) ? "New Request" : dto.Status,
                    CreatedAt = DateTime.UtcNow
                };

                _db.a_tblRequestAirSea.Add(entity);
                await _db.SaveChangesAsync();

                // Persist document references if provided
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
                return true;
            }
            catch (Exception ex)
            {
                await Helper.RollbackTransactionAsync(transaction, _logger, ex, "inserting Air/Sea request");
                return false;
            }
        }

        public async Task<bool> UpdateAsync(UpdateRequestAirSeaDto dto)
        {
            using var transaction = await _db.Database.BeginTransactionAsync();

            try
            {
                _logger.LogInformation("Updating air/sea request with ID: {RequestID}", dto.RequestID);

                var entity = await _db.a_tblRequestAirSea.FirstOrDefaultAsync(x => x.RequestID == dto.RequestID);

                if (entity is null)
                {
                    _logger.LogWarning("Air/Sea request with ID: {RequestID} not found.", dto.RequestID);
                    return false;
                }

                Helper.UpdateIfNotNull(v => entity.Status = v, dto.Status);
                Helper.UpdateIfNotNull(v => entity.RiderName = v, dto.RiderName);
                Helper.UpdateIfNotNull(v => entity.ItemPreparedAt = v, dto.ItemPreparedAt);
                Helper.UpdateIfNotNull(v => entity.ItemPreparedEndAt = v, dto.ItemPreparedEndAt);
                Helper.UpdateIfNotNull(v => entity.PreparedBy = v, dto.PreparedBy);
                Helper.UpdateIfNotNull(v => entity.MobileID = v, dto.MobileID);
                Helper.UpdateIfNotNull(v => entity.Remarks = v, dto.Remarks);

                // Set UpdatedAt each time UpdateAsync is used
                entity.UpdatedAt = DateTime.UtcNow;

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();
                _logger.LogInformation("Updated air/sea request with ID: {RequestID}", dto.RequestID);

                return true;
            }
            catch (Exception ex)
            {
                await Helper.RollbackTransactionAsync(transaction, _logger, ex, "updating Air/Sea request");
                return false;
            }
        }
    }
}
