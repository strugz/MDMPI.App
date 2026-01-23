using Microsoft.AspNetCore.Mvc;
using MDMPI.App.Core.Common.DTOs;
using MDMPI.App.Core.Common.Interfaces;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MDMPI.App.Api.Controllers.Common
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryRepository _repo;
        public CategoryController(ICategoryRepository repo) => _repo = repo;

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var cats = await _repo.GetAllCategoriesAsync();
            return Ok(cats);
        }
    }
}
