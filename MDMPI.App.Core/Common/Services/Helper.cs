using MDMPI.App.Core.Common.Entities;
using MDMPI.App.Core.Logistic.DTOs.RequestPullOutReturnPickUp;
using MDMPI.App.Core.Logistic.DTOs.RequestStandard;
using MDMPI.App.Core.Logistic.Entities;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDMPI.App.Core.Common.Services
{
    public  class Helper
    {
        public static void UpdateIfNotNull<T>(Action<T> setter, T? value)
        {
            if (value != null)
                setter(value);
        }

        public static async Task RollbackTransactionAsync(IDbContextTransaction transaction, ILogger logger, Exception ex, string contextMessage)
        {
            try
            {
                await transaction.RollbackAsync();
                logger.LogError(ex, "Transaction rolled back: {ContextMessage}", contextMessage);
            }
            catch (Exception rollbackEx)
            {
                logger.LogError(rollbackEx, "Error during transaction rollback: {ContextMessage}", contextMessage);
            }
        }

        public static IQueryable<RequestStandardModel> ApplyDateFilter(IQueryable<RequestStandardModel> query, RequestDateFilter filter)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            return filter switch
            {
                RequestDateFilter.Today => query.Where(r => r.RequestDeliveryDate == today),
                RequestDateFilter.Yesterday => query.Where(r => r.RequestDeliveryDate == today.AddDays(-1)),
                RequestDateFilter.Tomorrow => query.Where(r => r.RequestDeliveryDate == today.AddDays(1)),
                RequestDateFilter.FiveDaysAgo => query.Where(r => r.RequestDeliveryDate == today.AddDays(-5)),
                RequestDateFilter.ThirtyDaysAgo => query.Where(r => r.RequestDeliveryDate == today.AddDays(-30)),
                _ => query
            };
        }

        public static IQueryable<T> ApplyDateFilterAny<T>(
            IQueryable<T> query,
            RequestDateFilter filter,
            Func<T, DateOnly?> dateSelector)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            return filter switch
            {
                RequestDateFilter.Today => query.Where(x => dateSelector(x) == today),
                RequestDateFilter.Yesterday => query.Where(x => dateSelector(x) == today.AddDays(-1)),
                RequestDateFilter.Tomorrow => query.Where(x => dateSelector(x) == today.AddDays(1)),
                RequestDateFilter.FiveDaysAgo => query.Where(x => dateSelector(x) == today.AddDays(-5)),
                RequestDateFilter.ThirtyDaysAgo => query.Where(x => dateSelector(x) == today.AddDays(-30)),
                _ => query
            };
        }

        public static IQueryable<RequestStandardModel> ApplySorting(IQueryable<RequestStandardModel> query, string? sortBy, bool sortDesc)
        {
            return sortBy switch
            {
                "CreatedAt" => sortDesc ? query.OrderByDescending(r => r.RequestCreatedAt) : query.OrderBy(r => r.RequestCreatedAt),
                "DeliveryDate" => sortDesc ? query.OrderByDescending(r => r.RequestDeliveryDate) : query.OrderBy(r => r.RequestDeliveryDate),
                "RequestID" => sortDesc ? query.OrderByDescending(r => r.RequestID) : query.OrderBy(r => r.RequestID),
                _ => query.OrderByDescending(r => r.RequestCreatedAt)
            };
        }

        public static IQueryable<RequestStandardModel> ApplyPaging(IQueryable<RequestStandardModel> query, int page, int pageSize)
        {
            int skip = (page - 1) * pageSize;
            return query.Skip(skip).Take(pageSize);
        }

        // New: builds a RequestStandardModel from dto and provided id
        public static RequestStandardModel BuildRequestStandardModel(InsertRequestDto dto, long requestId)
        {
            return new RequestStandardModel
            {
                RequestID = requestId,
                RequestClientID = dto.RequestClientID,
                RequestShippingMethod = dto.RequestShippingMethod,
                RequestDeliveryTerms = dto.RequestDeliveryTerms,
                RequestDeliveryDate = DateOnly.TryParse(dto.RequestDeliveryDate, out var deliveryDate) ? deliveryDate : null,
                RequestPreference = dto.RequestPreference,
                RequestStatus = dto.RequestStatus,
                RequestBy = dto.RequestBy,
                RequestCreatedBy = dto.RequestCreatedBy,
                RequestCreatedAt = DateTime.UtcNow,
            };
        }

        public static RequestPullOutReturnPickUpModel BuilbRequestPullOutReturnPickUpModel(InsertRequestPullOutReturnPickUpDto dto, long requestid)
        {
            return new RequestPullOutReturnPickUpModel
            {
                RequestID = requestid,
                ClientID = dto.ClientID,
                FormCategoryID = dto.FormCategoryID,
                SlipNo = dto.SlipNo,
                IRRFNumber = dto.IRRFNumber,
                IRRFDate = dto.IRRFDate,
                ReasonForReturn = dto.ReasonForReturn,
                ItemCategoryID = dto.ItemCategoryID,
                PullOutDate = dto.PullOutDate,
                RequestStatus = dto.RequestStatus,
                CreatedAt = DateTime.UtcNow
            };
        }
    }
}
