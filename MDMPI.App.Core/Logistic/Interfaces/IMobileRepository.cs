using MDMPI.App.Core.Common.DTOs;

namespace MDMPI.App.Core.Logistic.Interfaces
{
    public interface IMobileRepository
    {
        Task<List<MobileDto>> GetAllMobilesAsync();
    }
}