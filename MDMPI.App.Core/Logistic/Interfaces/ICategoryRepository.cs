using MDMPI.App.Core.Common.DTOs;

namespace MDMPI.App.Core.Logistic.Interfaces
{
 public interface ICategoryRepository
 {
 Task<List<CategoryDto>> GetAllCategoriesAsync();
 }
}
