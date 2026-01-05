using MDMPI.App.Core.Common.DTOs;
using MDMPI.App.Core.Logistic.DTOs.RequestPullOutReturnPickUp;

namespace MDMPI.App.Core.Logistic.Interfaces
{
    public interface IRequestPullOutReturnPickUpRepository
    {
        Task<List<RequestPullOutReturnPickUpDto>> GetAllAsync(RequestQueryDto query);
        Task<RequestPullOutReturnPickUpDto?> InsertAsync(InsertRequestPullOutReturnPickUpDto dto);
        Task<bool> UpdateAsync(UpdateRequestPullOutReturnPickUpDto dto);
    }
}