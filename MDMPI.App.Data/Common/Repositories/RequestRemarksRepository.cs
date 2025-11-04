using MDMPI.App.Core.Common.DTOs;
using MDMPI.App.Core.Common.Entities;
using MDMPI.App.Core.Common.Services;
using MDMPI.App.Core.Logistic.Entities;
using MDMPI.App.Core.Logistic.Interfaces;
using MDMPI.App.Data.Logistic.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Reflection;

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
        public async Task<RemarksDto?> GetAllRemarks(long requestid)
        {
            _logger.LogInformation("Fetching remarks for RequestID: {RequestID}", requestid);

            var remarks = await _db.a_tblRequestRemarks
                .AsNoTracking()
                .Where(r => r.RequestID == requestid)
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
        public async Task<bool> InsertRemarkAndCancelRequestForStandardDeliveryAsync(long requestId, string remarks)
        {
            return await InsertRemarkAndCancelRequestAsync(_db.a_tblRequestStandardDelivery, requestId, remarks);
        }

        /// <summary>
        /// Inserts a remark for a request and cancels the request atomically (PullOut Return PickUp).
        /// </summary>
        public async Task<bool> InsertRemarkAndCancelRequestForPullOutReturnPickUp(long requestId, string remarks)
        {
            return await InsertRemarkAndCancelRequestAsync(_db.a_tblRequestPullOutReturnPickUp, requestId, remarks);
        }

        /// <summary>
        /// Inserts a remark for a request and cancels the request atomically (AirSea).
        /// </summary>
        public async Task<bool> InsertRemarkAndCancelRequestForAirSea(long requestId, string remarks)
        {
            return await InsertRemarkAndCancelRequestAsync(_db.a_tblRequestAirSea, requestId, remarks);
        }

        /// <summary>
        /// Inserts a remark for a request and cancels the request atomically (PickUp).
        /// </summary>
        public async Task<bool> InsertRemarkAndCancelRequestForPickUp(long requestId, string remarks)
        {
            return await InsertRemarkAndCancelRequestAsync(_db.a_tblRequestPickUpMDMPI, requestId, remarks);
        }

        /// <summary>
        /// Inserts a remark and cancels the request in a transaction.
        /// Handles entities whose primary key may be long or string by attempting to find by long first then by string.
        /// </summary>
        private async Task<bool> InsertRemarkAndCancelRequestAsync<TEntity>(DbSet<TEntity> dbSet, long requestId, string remarks) where TEntity : class
        {
            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                var remark = new RemarksModel
                {
                    RequestID = requestId,
                    Remarks = remarks,
                    Date = DateTime.UtcNow
                };

                _db.a_tblRequestRemarks.Add(remark);

                // Try find entity by long key first
                object? request = await dbSet.FindAsync(requestId);

                // If not found, try by string key (some tables use string PKs)
                if (request == null)
                {
                    request = await dbSet.FindAsync(requestId.ToString());
                }

                if (request == null)
                {
                    _logger.LogWarning("Request not found for ID: {RequestID}", requestId);
                    return false;
                }

                // Set RequestStatus or Status property to "Cancelled" using reflection (case-insensitive)
                var type = request.GetType();
                var prop = type.GetProperty("RequestStatus", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase)
                           ?? type.GetProperty("Status", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

                if (prop != null && prop.CanWrite)
                {
                    if (prop.PropertyType == typeof(string))
                    {
                        prop.SetValue(request, "Cancelled");
                    }
                    else
                    {
                        // Attempt to convert if property is not string (unlikely)
                        try
                        {
                            var converted = Convert.ChangeType("Cancelled", prop.PropertyType);
                            prop.SetValue(request, converted);
                        }
                        catch
                        {
                            _logger.LogWarning("Unable to set property {Property} on type {Type} to 'Cancelled'.", prop.Name, type.FullName);
                        }
                    }
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
