using MDMPI.App.Core.Collection.Interfaces;
using MDMPI.App.Core.Collection.Dtos;
using MDMPI.App.Core.Collection.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MDMPI.App.Core.Common.DTOs;

namespace MDMPI.App.Data.Collection.Repositories
{
    public class CollectionRepository : ICollectionTransactionDetailsRepository
    {
        private readonly AppDbContext _db;
        private readonly ILogger<CollectionRepository> _logger;

        public CollectionRepository(AppDbContext db, ILogger<CollectionRepository> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<List<CollectionTransactionDetailsDto>> GetAllAsync()
        {
            return await _db.a_tblCollectionTransactionDetails
                .AsNoTracking()
                .Include(x => x.Client)
                .Select(c => new CollectionTransactionDetailsDto
                {
                    ID = c.ID,
                    ReferenceCode = c.ReferenceCode,
                    ClientID = c.ClientID,
                    CollectorID = c.CollectorID,
                    CollectionDate = c.CollectionDate,
                    VisitType = c.VisitType,
                    created_at = c.created_at,
                    updated_at = c.updated_at,
                    Status = c.Status,
                    Client = c.Client == null ? null : new ACCMSTDto
                    {
                        ACCMID = c.Client.ACCMID,
                        ACCMSC = c.Client.ACCMSC,
                        ACCMNM = c.Client.ACCMNM,
                        ACCMBC = c.Client.ACCMBC,
                        ACCMAD = c.Client.ACCMAD,
                        ACCMPH = c.Client.ACCMPH,
                        ACCMEM = c.Client.ACCMEM,
                        ACCMWS = c.Client.ACCMWS,
                        ACCSTS = c.Client.ACCSTS,
                        ACCOWN = c.Client.ACCOWN
                    }
                })
                .ToListAsync();
        }

        public async Task<CollectionTransactionDetailsDto?> GetByIdAsync(long id)
        {
            var c = await _db.a_tblCollectionTransactionDetails
                .AsNoTracking()
                .Include(x => x.Client)
                .FirstOrDefaultAsync(x => x.ID == id);
            if (c == null) return null;

            return new CollectionTransactionDetailsDto
            {
                ID = c.ID,
                ReferenceCode = c.ReferenceCode,
                ClientID = c.ClientID,
                CollectorID = c.CollectorID,
                CollectionDate = c.CollectionDate,
                VisitType = c.VisitType,
                created_at = c.created_at,
                updated_at = c.updated_at,
                Status = c.Status,
                Client = c.Client == null ? null : new ACCMSTDto
                {
                    ACCMID = c.Client.ACCMID,
                    ACCMSC = c.Client.ACCMSC,
                    ACCMNM = c.Client.ACCMNM,
                    ACCMBC = c.Client.ACCMBC,
                    ACCMAD = c.Client.ACCMAD,
                    ACCMPH = c.Client.ACCMPH,
                    ACCMEM = c.Client.ACCMEM,
                    ACCMWS = c.Client.ACCMWS,
                    ACCSTS = c.Client.ACCSTS,
                    ACCOWN = c.Client.ACCOWN
                }
            };
        }

        public async Task<CollectionTransactionDetailsDto?> InsertAsync(CreateCollectionTransactionDetailsDto dto)
        {
            try
            {
                var model = new CollectionTransactionDetailsModel
                {
                    ReferenceCode = dto.ReferenceCode,
                    ClientID = dto.ClientID,
                    CollectorID = dto.CollectorID,
                    CollectionDate = dto.CollectionDate,
                    VisitType = dto.VisitType,
                    Status = dto.Status,
                    created_at = DateTime.UtcNow
                };

                _db.a_tblCollectionTransactionDetails.Add(model);
                await _db.SaveChangesAsync();

                return await GetByIdAsync(model.ID);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inserting collection transaction details");
                return null;
            }
        }

        public async Task<CollectionTransactionDetailsDto?> UpdateAsync(UpdateCollectionTransactionDetailsDto dto)
        {
            try
            {
                var model = await _db.a_tblCollectionTransactionDetails.FirstOrDefaultAsync(x => x.ID == dto.ID);
                if (model == null) return null;

                model.ReferenceCode = dto.ReferenceCode;
                model.Bank = dto.Bank;
                model.CheckNo = dto.CheckNo;
                model.CheckDate = dto.CheckDate;
                model.Amount = dto.Amount;
                model.SalesInvoiceReferenceForPayment = dto.SalesInvoiceReferenceForPayment;
                model.SummaryOfVisit = dto.SummaryOfVisit;
                model.Status = dto.Status;

                await _db.SaveChangesAsync();

                return await GetByIdAsync(model.ID);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating collection transaction details with ID {Id}", dto.ID);
                return null;
            }
        }

        public async Task<bool> DeleteAsync(long id)
        {
            var model = await _db.a_tblCollectionTransactionDetails.FirstOrDefaultAsync(x => x.ID == id);
            if (model == null) return false;

            _db.a_tblCollectionTransactionDetails.Remove(model);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
