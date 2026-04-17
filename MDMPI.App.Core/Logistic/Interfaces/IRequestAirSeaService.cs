using MDMPI.App.Core.Common.DTOs;
using MDMPI.App.Core.Logistic.DTOs.RequestAirSea;

namespace MDMPI.App.Core.Logistic.Interfaces
{
    public interface IRequestAirSeaService
    {
        Task<List<RequestAirSeaDto>> GetAllAsync(RequestQueryDto query);
        Task<RequestAirSeaDto?> InsertAsync(InsertRequestAirSeaDto dto);
        Task<bool> UpdateAsync(UpdateRequestAirSeaDto dto);
        Task<List<RequestAirSeaHistoryDto>> GetHistoryAsync(long requestId);
    }
}
