using MDMPI.App.Core.Common.DTOs;

namespace MDMPI.App.Core.Common.Interfaces
{
 public interface ICategoryRepository
 {
 Task<List<CategoryDto>> GetAllCategoriesAsync();
 }
}
