using MDMPI.App.Core.Common.DTOs;
using MDMPI.App.Core.Logistic.DTOs.RequestBackload;
using MDMPI.App.Core.Logistic.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MDMPI.App.Api.Controllers.Logistic
{
    [Route("api/[controller]")]
    [ApiController]
    public class RequestBackloadController : ControllerBase
    {
        private readonly IRequestBackloadRepository _repository;
        private readonly ILogger<RequestBackloadController> _logger;

        public RequestBackloadController(IRequestBackloadRepository repository, ILogger<RequestBackloadController> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] RequestDateFilter dateFilter = RequestDateFilter.All, [FromQuery] RequestStatusFilter statusFilter = RequestStatusFilter.All)
        {
            var query = new RequestQueryDto
            {
                Page = page,
                PageSize = pageSize,
                DateFilter = dateFilter,
                StatusFilter = statusFilter
            };

            var result = await _repository.GetAllAsync(query);
            if (result == null)
                return NotFound();
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Insert([FromBody] InsertRequestBackloadDto dto)
        {
            var inserted = await _repository.InsertAsync(dto);
            if (inserted is null)
                return BadRequest("Insert failed.");
            return CreatedAtAction(nameof(GetAll), new { id = inserted.BackLoadID }, inserted);
        }
    }
}
