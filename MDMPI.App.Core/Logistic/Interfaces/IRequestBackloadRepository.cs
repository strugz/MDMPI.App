using MDMPI.App.Core.Common.DTOs;
using MDMPI.App.Core.Logistic.DTOs.RequestBackload;

namespace MDMPI.App.Core.Logistic.Interfaces
{
    public interface IRequestBackloadRepository
    {
        Task<List<RequestBackloadDto>> GetAllAsync(RequestQueryDto query);
        Task<RequestBackloadDto?> InsertAsync(InsertRequestBackloadDto dto);
    }
}
