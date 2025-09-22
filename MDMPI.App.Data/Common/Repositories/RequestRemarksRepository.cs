using MDMPI.App.Core.Common.DTOs;
using MDMPI.App.Core.Common.Entities;
using MDMPI.App.Core.Common.Services;
using MDMPI.App.Core.Logistic.Entities;
using MDMPI.App.Core.Logistic.Interfaces;
using MDMPI.App.Data.Logistic.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MDMPI.App.Data.Common.Repositories
{
    public class RequestRemarksRepository : IRequestRemarksRepository
    {
        private readonly AppDbContext _db;
        private readonly ILogger<RequestRepository> _logger;

        public RequestRemarksRepository(AppDbContext db, ILogger<RequestRepository> logger)
        {
            _db = db;
            _logger = logger;
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
                    RequestID = r.RequestID.ToString(),
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
        /// Inserts a remark for a request and cancels the request atomically (Standard Delivery).
        /// </summary>
        public async Task<bool> InsertRemarkAndCancelRequestForStandardDeliveryAsync(string requestId, string remarks)
        {
            return await InsertRemarkAndCancelRequestAsync(_db.a_tblRequestStandardDelivery, requestId, remarks);
        }

        /// <summary>
        /// Inserts a remark for a request and cancels the request atomically (PullOut Return PickUp).
        /// </summary>
        public async Task<bool> InsertRemarkAndCancelRequestForPullOutReturnPickUp(string requestId, string remarks)
        {
            return await InsertRemarkAndCancelRequestAsync(_db.a_tblRequestPullOutReturnPickUp, requestId, remarks);
        }

        /// <summary>
        /// Inserts a remark and cancels the request in a transaction.
        /// </summary>
        private async Task<bool> InsertRemarkAndCancelRequestAsync<TEntity>(DbSet<TEntity> dbSet, string requestId, string remarks) where TEntity : class
        {
            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                var remark = new RemarksModel
                {
                    RequestID = long.Parse(requestId),
                    Remarks = remarks,
                    Date = DateTime.UtcNow
                };

                _db.a_tblRequestRemarks.Add(remark);

                var request = await dbSet.FindAsync(long.Parse(requestId));
                if (request == null)
                {
                    _logger.LogWarning("Request not found for ID: {RequestID}", requestId);
                    return false;
                }

                // Set RequestStatus property to "Cancelled" using reflection
                var prop = request.GetType().GetProperty("RequestStatus");
                if (prop != null && prop.CanWrite)
                {
                    prop.SetValue(request, "Cancelled");
                }

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
