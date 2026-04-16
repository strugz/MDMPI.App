using MDMPI.App.Core.Common.DTOs;

namespace MDMPI.App.Core.Common.Interfaces
{
    public interface IMobileService
    {
        Task<List<MobileDto>> GetAllMobilesAsync();
    }
}
