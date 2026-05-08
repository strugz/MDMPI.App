using MDMPI.App.Core.Common.DTOs;

namespace MDMPI.App.Core.Common.Interfaces
{
    public interface IClientLookupRepository
    {
        Task<Dictionary<string, ACCMSTDto>> GetByIdsAsync(IEnumerable<string> clientIds);
    }
}
