
using MDMPI.App.Core.Common.DTOs;
using MDMPI.App.Core.Common.DTOs.Item;
using MDMPI.App.Core.Common.Entities.Item;
using MDMPI.App.Core.Common.Interfaces;
using MDMPI.App.Core.Common.Services;
using MDMPI.App.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MDMPI.App.Data.Common.Repositories
{
    public class ItemRepository : IItemRepository
    {
        private readonly AppDbContext _db;
        private readonly ILogger<ItemRepository> _logger;
        private readonly IItemIdGenerator _itemIdGenerator;
        private readonly IBatchIdGenerator _batchIdGenerator;

        public ItemRepository(
            AppDbContext db,
            ILogger<ItemRepository> logger,
            IItemIdGenerator itemIdGenerator,
            IBatchIdGenerator batchIdGenerator)
        {
            _db = db;
            _logger = logger;
            _itemIdGenerator = itemIdGenerator;
            _batchIdGenerator = batchIdGenerator;
        }

        public async Task<List<FetchItemDto>> GetItemsByRequestIdAsync(long requestId)
        {
            var items = await _db.a_tblRequestStandardItem
                .AsNoTracking()
                .Where(i => i.RequestID == requestId)
                .Select(i => new FetchItemDto
                {
                    RequestItemID = i.RequestItemID,
                    RequestID = i.RequestID.ToString(),
                    ItemCode = i.ItemCode,
                    Description = i.Description,
                    Qty = i.Qty,
                    Unit = i.Unit,
                    Batch = i.Batch!.Select(b => new FetchItemBatchDto
                    {
                        RequestItemBatchID = b.RequestItemBatchID,
                        RequestItemID = b.RequestItemID,
                        BatchSerial = b.BatchSerial,
                        BatchQuantity = b.BatchQuantity,
                        ExpiryDate = b.ExpiryDate.HasValue ? b.ExpiryDate.Value.ToString("yyyy-MM-dd") : null
                    }).ToList()
                })
                .ToListAsync();

            return items;
        }

        public async Task<bool> InsertItemsAsync(long requestId, List<InsertItemDto> items)
        {
            if (items is null || items.Count == 0)
                return true;

            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                var itemModels = new List<ItemModel>(items.Count);
                foreach (var dto in items)
                {
                    var generatedItemId = await _itemIdGenerator.GenerateAsync();
                    var model = new ItemModel
                    {
                        RequestItemID = generatedItemId,
                        RequestID = requestId,
                        ItemCode = dto.ItemCode,
                        Description = dto.Description,
                        Qty = dto.Qty,
                        Unit = dto.Unit
                    };
                    itemModels.Add(model);
                }

                _db.a_tblRequestStandardItem.AddRange(itemModels);
                await _db.SaveChangesAsync();

                var batchModels = new List<ItemBatchModel>();
                for (int idx = 0; idx < items.Count; idx++)
                {
                    var dtoItem = items[idx];
                    var savedItem = itemModels[idx];

                    if (dtoItem.Batch is null || dtoItem.Batch.Count == 0)
                        continue;

                    foreach (var b in dtoItem.Batch)
                    {
                        DateOnly? expiry = null;
                        if (!string.IsNullOrWhiteSpace(b.ExpiryDate) && DateOnly.TryParse(b.ExpiryDate, out var d))
                            expiry = d;

                        var generatedBatchId = await _batchIdGenerator.GenerateAsync();
                        batchModels.Add(new ItemBatchModel
                        {
                            RequestItemBatchID = generatedBatchId,
                            RequestItemID = savedItem.RequestItemID,
                            BatchSerial = b.BatchSerial,
                            BatchQuantity = b.BatchQuantity,
                            ExpiryDate = expiry
                        });
                    }
                }

                if (batchModels.Count > 0)
                {
                    _db.a_tblRequestStandardItemBatch.AddRange(batchModels);
                    await _db.SaveChangesAsync();
                }

                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await TransactionHelper.RollbackTransactionAsync(transaction, _logger, ex, $"Error inserting items for RequestID: {requestId}");
                return false;
            }
        }

        public async Task<bool> UpdateItemsAsync(long requestId, List<UpdateItemDto> items)
        {
            if (items is null || items.Count == 0)
                return true;

            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                foreach (var dto in items)
                {
                    // Prefer precise RequestItemID for updates; skip if missing.
                    if (!dto.RequestItemID.HasValue)
                        continue;

                    var item = await _db.a_tblRequestStandardItem
                        .FirstOrDefaultAsync(i => i.RequestItemID == dto.RequestItemID.Value && i.RequestID == requestId);

                    if (item is null)
                        continue;

                    QueryFilterHelper.UpdateIfNotNull(v => item.ItemCode = v, dto.ItemCode);
                    QueryFilterHelper.UpdateIfNotNull(v => item.Description = v, dto.Description);
                    QueryFilterHelper.UpdateIfNotNull(v => item.Qty = v, dto.Qty);
                    QueryFilterHelper.UpdateIfNotNull(v => item.Unit = v, dto.Unit);

                    // Handle batches for this item
                    if (dto.Batch is not null)
                    {
                        var existingBatches = await _db.a_tblRequestStandardItemBatch
                            .Where(b => b.RequestItemID == item.RequestItemID)
                            .ToListAsync();

                        var incomingIds = dto.Batch
                            .Where(b => b.RequestItemBatchID.HasValue)
                            .Select(b => b.RequestItemBatchID!.Value)
                            .ToHashSet();

                        // Remove batches that are no longer sent in DTO
                        var toRemove = existingBatches
                            .Where(b => !incomingIds.Contains(b.RequestItemBatchID))
                            .ToList();

                        if (toRemove.Count > 0)
                            _db.a_tblRequestStandardItemBatch.RemoveRange(toRemove);

                        // Upsert incoming batches
                        foreach (var bDto in dto.Batch)
                        {
                            if (bDto.RequestItemBatchID.HasValue)
                            {
                                var batch = existingBatches.FirstOrDefault(x => x.RequestItemBatchID == bDto.RequestItemBatchID.Value);
                                if (batch is null)
                                    continue;

                                QueryFilterHelper.UpdateIfNotNull(v => batch.BatchSerial = v, bDto.BatchSerial);
                                QueryFilterHelper.UpdateIfNotNull(v => batch.BatchQuantity = v, bDto.BatchQuantity);
                                if (!string.IsNullOrWhiteSpace(bDto.ExpiryDate))
                                {
                                    if (DateOnly.TryParse(bDto.ExpiryDate, out var d))
                                        batch.ExpiryDate = d;
                                }
                                else
                                {
                                    // allow clearing expiry by sending null/empty
                                    batch.ExpiryDate = null;
                                }
                            }
                            else
                            {
                                // new batch, insert
                                DateOnly? expiry = null;
                                if (!string.IsNullOrWhiteSpace(bDto.ExpiryDate) && DateOnly.TryParse(bDto.ExpiryDate, out var d))
                                    expiry = d;

                                var genBatchId = await _batchIdGenerator.GenerateAsync();
                                _db.a_tblRequestStandardItemBatch.Add(new ItemBatchModel
                                {
                                    RequestItemBatchID = genBatchId,
                                    RequestItemID = item.RequestItemID,
                                    BatchSerial = bDto.BatchSerial,
                                    BatchQuantity = bDto.BatchQuantity,
                                    ExpiryDate = expiry
                                });
                            }
                        }
                    }
                }

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await TransactionHelper.RollbackTransactionAsync(transaction, _logger, ex, $"Error updating items for RequestID: {requestId}");
                return false;
            }
        }
    }
}