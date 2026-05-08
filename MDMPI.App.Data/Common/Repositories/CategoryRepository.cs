using MDMPI.App.Core.Common.DTOs;
using MDMPI.App.Core.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MDMPI.App.Data.Common.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly PostgreSqlAppDbContext _db;
        public CategoryRepository(PostgreSqlAppDbContext db) => _db = db;

        public async Task<List<CategoryDto>> GetAllCategoriesAsync()
        {
            var categories = await _db.a_tblCategory
            .Select(c => new CategoryDto
            {
                ID = c.ID,
                Category = c.Category,
                Type = c.Type
            })
            .ToListAsync();

            return categories;
        }
    }
}
