using MDMPI.App.Core.Common.DTOs;

namespace MDMPI.App.Core.Logistic.Interfaces
{
    public interface IRequestRemarksRepository
    {
        Task<RemarksDto?> GetAllRemarks(string requestid);
        Task<bool> InsertRemarkAndCancelRequestForStandardDeliveryAsync(string requestId, string remarks);
        Task<bool> InsertRemarkAndCancelRequestForPullOutReturnPickUp(string requestId, string remarks);
    }
}