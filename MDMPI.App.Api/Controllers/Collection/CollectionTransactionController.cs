using MDMPI.App.Api.Models;
using MDMPI.App.Core.Collection.Dtos;
using MDMPI.App.Core.Collection.Interfaces;
using MDMPI.App.Core.Logistic.Interfaces;
using MDMPI.App.Core.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MDMPI.App.Api.Controllers.Collection
{
    [Route("api/[controller]")]
    [ApiController]
    public class CollectionTransactionController : ControllerBase
    {
        private readonly ICollectionTransactionDetailsRepository _repo;
        private readonly IImagePathTypeRepository _imagePathTypeRepository;
        public CollectionTransactionController(ICollectionTransactionDetailsRepository repo, IImagePathTypeRepository imagePathTypeRepository)
        {
            _repo = repo;
            _imagePathTypeRepository = imagePathTypeRepository;
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

        [HttpGet("image")]
        public async Task<ActionResult> GetImage([FromQuery] string requestID, [FromQuery] string type)
        {
            if (string.IsNullOrEmpty(requestID) || string.IsNullOrEmpty(type))
            {
                return BadRequest("RequestID and Type are required.");
            }
            var result = await _imagePathTypeRepository.GetRequestImage(requestID, type);
            if (result == null)
            {
                return NotFound("Image not found.");
            }
            return Ok(new { path = result });
        }

        [HttpPost("upload-image")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> UploadImage([FromForm] UploadImageRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var image = dto.Image;
            if (image == null || image.Length == 0)
            {
                return BadRequest("No image file provided.");
            }

            using var ms = new MemoryStream();
            await image.CopyToAsync(ms);
            var imageBytes = ms.ToArray();


            var result = await _imagePathTypeRepository.UploadImageAsync(imageBytes, dto.RequestID!, dto.Type!);

            if (result == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to upload image.");
            }

            return Ok(new { path = result });
        }
    }
}
