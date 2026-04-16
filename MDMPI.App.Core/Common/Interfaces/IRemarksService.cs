using MDMPI.App.Core.Common.DTOs;

namespace MDMPI.App.Core.Common.Interfaces
{
    public interface IRemarksService
    {
        Task<RemarksDto?> GetAllRemarks(long requestId);
        Task<bool> CancelStandardDeliveryAsync(long requestId, string user, string remarks);
        Task<bool> CancelPullOutReturnPickUpAsync(long requestId, string user, string remarks);
        Task<bool> CancelAirSeaAsync(long requestId, string user, string remarks);
        Task<bool> CancelPickUpAsync(long requestId, string user, string remarks);
    }
}
