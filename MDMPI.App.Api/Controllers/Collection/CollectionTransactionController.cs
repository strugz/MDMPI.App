using Microsoft.AspNetCore.Mvc;
using MDMPI.App.Core.Collection.Interfaces;
using MDMPI.App.Core.Collection.Dtos;

namespace MDMPI.App.Api.Controllers.Collection
{
    [Route("api/[controller]")]
    [ApiController]
    public class CollectionTransactionController : ControllerBase
    {
        private readonly ICollectionTransactionDetailsRepository _repo;

        public CollectionTransactionController(ICollectionTransactionDetailsRepository repo)
        {
            _repo = repo;
        }

        /// <summary>
        /// Retrieves all collection transaction records.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<CollectionTransactionDetailsDto>>> GetAll()
        {
            var items = await _repo.GetAllAsync();
            return Ok(items);
        }

        /// <summary>
        /// Retrieves a single collection transaction by ID.
        /// </summary>
        [HttpGet("{id}", Name = "GetById")]
        public async Task<ActionResult<CollectionTransactionDetailsDto>> GetById(long id)
        {
            var item = await _repo.GetByIdAsync(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        /// <summary>
        /// Creates a new collection transaction record.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<CollectionTransactionDetailsDto>> Create([FromBody] CreateCollectionTransactionDetailsDto dto)
        {
            if (dto == null) return BadRequest();

            var created = await _repo.InsertAsync(dto);
            if (created == null) return BadRequest("Unable to create record");

            return CreatedAtRoute("GetById", new { id = created.ID }, created);
        }

        /// <summary>
        /// Updates an existing collection transaction record.
        /// </summary>
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateCollectionTransactionDetailsDto dto)
        {
            if (dto == null) return BadRequest();

            var ok = await _repo.UpdateAsync(dto);
            if (ok == null) return NotFound();

            return NoContent();
        }
    }
}
