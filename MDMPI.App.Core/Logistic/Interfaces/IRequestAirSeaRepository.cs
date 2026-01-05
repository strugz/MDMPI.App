using System.Threading.Tasks;
using System.Collections.Generic;
using MDMPI.App.Core.Common.DTOs;
using MDMPI.App.Core.Logistic.DTOs.RequestAirSea;

namespace MDMPI.App.Core.Logistic.Interfaces
{
    public interface IRequestAirSeaRepository
    {
        Task<List<RequestAirSeaDto>> GetAllAsync(RequestQueryDto query);
        Task<RequestAirSeaDto?> InsertAsync(InsertRequestAirSeaDto dto);
        Task<bool> UpdateAsync(UpdateRequestAirSeaDto dto);
    }
}
