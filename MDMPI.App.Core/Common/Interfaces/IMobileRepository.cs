using MDMPI.App.Core.Common.DTOs;

namespace MDMPI.App.Core.Common.Interfaces
{
    public interface IMobileRepository
    {
        Task<List<MobileDto>> GetAllMobilesAsync();
    }
}