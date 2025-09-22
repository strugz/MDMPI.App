using MDMPI.App.Core.Logistic.DTOs.RequestPullOutReturnPickUp;
using MDMPI.App.Core.Logistic.DTOs.RequestStandard;

namespace MDMPI.App.Core.Logistic.Interfaces
{
    public interface IRequestPullOutReturnPickUpRepository
    {
        Task<List<DisplayRequestPullOutReturnPickUpDto>> GetAllAsync(RequestQueryDto query);
        Task<bool> InsertAsync(InsertRequestPullOutReturnPickUpDto dto);
        Task<bool> UpdateAsync(UpdateRequestPullOutReturnPickUpDto dto);
    }
}