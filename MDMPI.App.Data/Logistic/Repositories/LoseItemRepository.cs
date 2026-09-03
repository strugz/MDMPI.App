using MDMPI.App.Core.Logistic.DTOs.LoseItem;
using MDMPI.App.Core.Logistic.Entities;
using MDMPI.App.Core.Logistic.Interfaces;
using MDMPI.App.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MDMPI.App.Data.Logistic.Repositories
{
    public class LoseItemRepository : ILoseItemRepository
    {
        private readonly PostgreSqlAppDbContext _db;
        private readonly ILogger<LoseItemRepository> _logger;

        public LoseItemRepository(PostgreSqlAppDbContext db, ILogger<LoseItemRepository> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<bool> PullOutRequestExistsAsync(long requestId)
        {
            return await _db.a_tblRequestPullOutReturnPickUp
                .AsNoTracking()
                .AnyAsync(r => r.RequestID == requestId);
        }

        public async Task<List<FetchLoseItemDto>> GetByRequestIdAsync(long requestId)
        {
            return await _db.a_tblLoseItem
                .AsNoTracking()
                .Where(i => i.RequestID == requestId)
                .OrderBy(i => i.LoseItemID)
                .Select(i => new FetchLoseItemDto
                {
                    LoseItemID = i.LoseItemID,
                    RequestID = i.RequestID.ToString(),
                    ItemCode = i.ItemCode,
                    Remarks = i.Remarks,
                    CreatedAt = i.CreatedAt.HasValue
                        ? i.CreatedAt.Value.ToString("yyyy-MM-dd HH:mm:ss")
                        : null
                })
                .ToListAsync();
        }

        public async Task<bool> ReplaceForRequestAsync(long requestId, List<InsertLoseItemDto> items)
        {
            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                var existing = await _db.a_tblLoseItem
                    .Where(i => i.RequestID == requestId)
                    .ToListAsync();
                if (existing.Count > 0)
                    _db.a_tblLoseItem.RemoveRange(existing);

                if (items is not null && items.Count > 0)
                {
                    var now = DateTime.UtcNow;
                    _db.a_tblLoseItem.AddRange(items.Select(dto => new LoseItemModel
                    {
                        RequestID = requestId,
                        ItemCode = dto.ItemCode,
                        Remarks = dto.Remarks,
                        CreatedAt = now
                    }));
                }

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await TransactionHelper.RollbackTransactionAsync(transaction, _logger, ex, $"Error saving lost items for RequestID: {requestId}");
                return false;
            }
        }
    }
}
