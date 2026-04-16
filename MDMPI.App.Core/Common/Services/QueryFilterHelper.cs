using MDMPI.App.Core.Common.DTOs;
using MDMPI.App.Core.Common.Entities;
using MDMPI.App.Core.Logistic.DTOs.RequestPullOutReturnPickUp;
using MDMPI.App.Core.Logistic.DTOs.RequestStandard;
using MDMPI.App.Core.Logistic.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace MDMPI.App.Core.Common.Services
{
    public class QueryFilterHelper
    {
        public static void UpdateIfNotNull<T>(Action<T> setter, T? value)
        {
            if (value != null)
                setter(value);
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

        // Changed to accept an Expression so EF can translate the composed expression
        public static IQueryable<T> ApplyDateFilterAny<T>(
            IQueryable<T> query,
            RequestDateFilter filter,
            Expression<Func<T, DateOnly?>> dateSelector)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);

            // helper to build a lambda comparing the selector to a specific DateOnly value
            Expression<Func<T, bool>> BuildEqualsExpression(DateOnly compareDate)
            {
                var parameter = dateSelector.Parameters[0];
                var body = dateSelector.Body; // expression returning DateOnly?

                var constant = Expression.Constant(compareDate, typeof(DateOnly));

                var hasValue = Expression.Property(body, "HasValue");
                var value = Expression.Property(body, "Value");
                var equal = Expression.Equal(value, constant);
                var andAlso = Expression.AndAlso(hasValue, equal);

                return Expression.Lambda<Func<T, bool>>(andAlso, parameter);
            }

            return filter switch
            {
                RequestDateFilter.Today => query.Where(BuildEqualsExpression(today)),
                RequestDateFilter.Yesterday => query.Where(BuildEqualsExpression(today.AddDays(-1))),
                RequestDateFilter.Tomorrow => query.Where(BuildEqualsExpression(today.AddDays(1))),
                RequestDateFilter.FiveDaysAgo => query.Where(BuildEqualsExpression(today.AddDays(-5))),
                RequestDateFilter.ThirtyDaysAgo => query.Where(BuildEqualsExpression(today.AddDays(-30))),
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
                FormCategoryID = dto.FormCategoryID,
                ItemCategoryID = dto.ItemCategoryID,
                RequestShippingMethod = dto.RequestShippingMethod,
                RequestDeliveryTerms = dto.RequestDeliveryTerms,
                RequestDeliveryDate = DateOnly.TryParse(dto.RequestDeliveryDate, out var deliveryDate) ? deliveryDate : null,
                RequestPreference = dto.RequestPreference,
                RequestStatus = dto.RequestStatus,
                RequestBy = dto.RequestBy,
                RequestCreatedBy = dto.RequestCreatedBy,
                RequestCreatedAt = DateTime.UtcNow,
                UpdatedBy = dto.UpdatedBy
            };
        }

        public static RequestPullOutReturnPickUpModel BuilbRequestPullOutReturnPickUpModel(InsertRequestPullOutReturnPickUpDto dto, long requestid)
        {
            return new RequestPullOutReturnPickUpModel
            {
                RequestID = requestid,
                ClientID = dto.ClientID,
                ClientContactPerson = dto.ClientContactPerson,
                FormCategoryID = dto.FormCategoryID,
                SlipNo = dto.SlipNo,
                IRRFNumber = dto.IRRFNumber,
                IRRFDate = dto.IRRFDate,
                ReasonForReturn = dto.ReasonForReturn,
                ItemCategoryID = dto.ItemCategoryID,
                // PullOutDate is DateOnly? in model and DTO
                PullOutDate = dto.PullOutDate,
                RequestStatus = dto.RequestStatus,
                CreatedBy = dto.CreatedBy,
                RequestedBy = dto.RequestedBy,
                CreatedAt = DateTime.UtcNow
            };
        }
    }
}
