using MDMPI.App.Core.Common.DTOs;
using MDMPI.App.Core.Common.Interfaces;

namespace MDMPI.App.Core.Common.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _repository;

        public CategoryService(ICategoryRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<CategoryDto>> GetAllCategoriesAsync()
        {
            return await _repository.GetAllCategoriesAsync();
        }
    }
}
