using MDMPI.App.Core.Common.DTOs;
using MDMPI.App.Core.Logistic.DTOs.RequestStandard;

namespace MDMPI.App.Core.Logistic.Interfaces
{
    public interface IRequestRepository
    {
        Task<List<RequestStandardDto>> GetAllRequestsAsync(RequestQueryDto query);
        Task<RequestStandardDto?> InsertRequest(InsertRequestDto dto);
        Task<bool> UpdateRequest(UpdateRequestDto dto);
    }
}