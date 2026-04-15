using MDMPI.App.Common.Utilities;
using MDMPI.App.Core.Common.DTOs;
using MDMPI.App.Core.Common.Interfaces;
using MDMPI.App.Core.Logistic.DTOs.RequestBackload;
using MDMPI.App.Core.Logistic.Entities;
using MDMPI.App.Core.Logistic.Interfaces;
using MDMPI.App.Core.Common.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MDMPI.App.Data.Logistic.Repositories
{
    public class RequestBackloadRepository : IRequestBackloadRepository
    {
        private readonly AppDbContext _db;
        private readonly ILogger<RequestBackloadRepository> _logger;
        private readonly IBackloadIdGenerator _backloadIdGenerator;

        public RequestBackloadRepository(AppDbContext db, ILogger<RequestBackloadRepository> logger, IBackloadIdGenerator backloadIdGenerator)
        {
            _db = db;
            _logger = logger;
            _backloadIdGenerator = backloadIdGenerator;
        }

        public async Task<List<RequestBackloadDto>> GetAllAsync(RequestQueryDto query)
        {
            _logger.LogInformation("Fetching backload records with query: {@Query}", query);

            var q = _db.a_tblRequestBackload.AsNoTracking().AsQueryable();

            // Apply date filtering on DateReported (DateTime?) if provided
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

                q = q.Where(r => r.DateReported.HasValue && r.DateReported.Value >= start && r.DateReported.Value < end);
            }

            var result = await q.Select(r => new RequestBackloadDto
            {
                BackLoadID = r.BackLoadID,
                RequestID = r.RequestID,
                Remarks = r.Remarks,
                DateReported = r.DateReported
            }).ToListAsync();

            _logger.LogInformation("Fetched {Count} backload records.", result.Count);
            return result;
        }

        public async Task<RequestBackloadDto?> InsertAsync(InsertRequestBackloadDto dto)
        {
            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                _logger.LogInformation("Inserting backload for RequestID: {RequestID}", dto.RequestID);

                var backloadId = await _backloadIdGenerator.GenerateAsync();

                var model = new RequestBackloadModel
                {
                    BackLoadID = backloadId,
                    RequestID = dto.RequestID,
                    Remarks = dto.Remarks,
                    DateReported = dto.DateReported
                };

                _db.a_tblRequestBackload.Add(model);
                await _db.SaveChangesAsync();

                await transaction.CommitAsync();

                var inserted = new RequestBackloadDto
                {
                    BackLoadID = model.BackLoadID,
                    RequestID = model.RequestID,
                    Remarks = model.Remarks,
                    DateReported = model.DateReported
                };

                _logger.LogInformation("Inserted backload with ID: {BackLoadID}", model.BackLoadID);
                return inserted;
            }
            catch (Exception ex)
            {
                await Helper.RollbackTransactionAsync(transaction, _logger, ex, "inserting backload record");
                return null;
            }
        }
    }
}