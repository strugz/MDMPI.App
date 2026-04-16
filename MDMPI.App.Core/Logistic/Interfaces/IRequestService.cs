using MDMPI.App.Core.Common.DTOs;
using MDMPI.App.Core.Common.DTOs.Item;
using MDMPI.App.Core.Logistic.DTOs.RequestStandard;

namespace MDMPI.App.Core.Logistic.Interfaces
{
    public interface IRequestService
    {
        Task<List<RequestStandardDto>> GetAllRequestsAsync(RequestQueryDto query);
        Task<RequestStandardDto?> CreateRequestWithItemsAsync(InsertRequestDto dto);
        Task<bool> UpdateRequestAsync(UpdateRequestDto dto);
        Task<List<RequestStandardHistoryDto>> GetRequestHistoryAsync(long requestId);
    }
}
