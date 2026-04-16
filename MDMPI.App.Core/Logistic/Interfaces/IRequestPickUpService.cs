using MDMPI.App.Core.Common.DTOs;
using MDMPI.App.Core.Logistic.DTOs.RequestPickUp;

namespace MDMPI.App.Core.Logistic.Interfaces
{
    public interface IRequestPickUpService
    {
        Task<List<RequestPickUpDto>> GetAllAsync(RequestQueryDto query);
        Task<RequestPickUpDto?> InsertAsync(InsertRequestPickUpDto dto);
        Task<bool> UpdateAsync(UpdateRequestPickUpDto dto);
    }
}
