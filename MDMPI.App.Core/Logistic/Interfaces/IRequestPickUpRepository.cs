using MDMPI.App.Core.Common.DTOs;
using MDMPI.App.Core.Logistic.DTOs.RequestPickUp;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MDMPI.App.Core.Logistic.Interfaces
{
    public interface IRequestPickUpRepository
    {
        Task<List<RequestPickUpDto>> GetAllAsync(RequestQueryDto query);
        Task<RequestPickUpDto?> InsertAsync(InsertRequestPickUpDto dto);
        Task<bool> UpdateAsync(UpdateRequestPickUpDto dto);
    }
}
