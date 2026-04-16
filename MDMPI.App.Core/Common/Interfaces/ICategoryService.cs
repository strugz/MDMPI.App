using MDMPI.App.Core.Common.DTOs;

namespace MDMPI.App.Core.Common.Interfaces
{
    public interface ICategoryService
    {
        Task<List<CategoryDto>> GetAllCategoriesAsync();
    }
}
