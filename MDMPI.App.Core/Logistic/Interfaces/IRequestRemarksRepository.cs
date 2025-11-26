using MDMPI.App.Core.Common.DTOs;

namespace MDMPI.App.Core.Logistic.Interfaces
{
    public interface IRequestRemarksRepository
    {
        Task<RemarksDto?> GetAllRemarks(long requestid);
        Task<bool> InsertRemarkAndCancelRequestForStandardDeliveryAsync(long requestId, string user, string remarks);
        Task<bool> InsertRemarkAndCancelRequestForPullOutReturnPickUp(long requestId, string user, string remarks);
        Task<bool> InsertRemarkAndCancelRequestForAirSea(long requestId, string user, string remarks);
        Task<bool> InsertRemarkAndCancelRequestForPickUp(long requestId, string user, string remarks);
    }
}