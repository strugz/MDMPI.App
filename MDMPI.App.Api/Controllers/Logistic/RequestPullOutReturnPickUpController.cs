using MDMPI.App.Api.Models;
using MDMPI.App.Core.Common.DTOs;
using MDMPI.App.Core.Common.Interfaces;
using MDMPI.App.Core.Logistic.DTOs.RequestPullOutReturnPickUp;
using MDMPI.App.Core.Logistic.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MDMPI.App.Api.Controllers.Logistic
{
    [Route("api/[controller]")]
    [ApiController]
    public class RequestPullOutReturnPickUpController : ControllerBase
    {
        private readonly IRequestPullOutReturnPickUpRepository _repository;
        private readonly ILogger<RequestPullOutReturnPickUpController> _logger;
        private readonly IRequestRemarksRepository _requestRemarksRepository;
        private readonly IImagePathTypeRepository _imagePathTypeRepository;

        public RequestPullOutReturnPickUpController(IRequestPullOutReturnPickUpRepository repository, ILogger<RequestPullOutReturnPickUpController> logger, IRequestRemarksRepository requestRemarksRepository, IImagePathTypeRepository imagePathTypeRepository)
        {
            _repository = repository;
            _logger = logger;
            _requestRemarksRepository = requestRemarksRepository;
            _imagePathTypeRepository = imagePathTypeRepository;
        }

        [HttpGet]
        public async Task<ActionResult> GetRequestAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] RequestDateFilter dateFilter = RequestDateFilter.All, [FromQuery] RequestStatusFilter statusFilter = RequestStatusFilter.All)
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
            {
                return NotFound();
            }
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Insert([FromBody] InsertRequestPullOutReturnPickUpDto dto)
        {
            var inserted = await _repository.InsertAsync(dto);
            if (inserted is null)
                return BadRequest("Insert failed.");
            return CreatedAtAction(nameof(GetRequestAll), new { id = inserted.RequestID }, inserted);
        }

        [HttpPatch]
        public async Task<IActionResult> Update([FromBody] UpdateRequestPullOutReturnPickUpDto dto)
        {
            if (dto.RequestID <= 0)
            {
                return BadRequest("RequestID is required and must be greater than zero.");
            }

            var success = await _repository.UpdateAsync(dto);
            if (!success)
            {
                return NotFound("Request not found or update failed.");
            }
            return Ok("Request updated successfully.");
        }

        [HttpGet("cancel/{requestid}")]
        public async Task<ActionResult> GetCancelledRemarks(long requestid)
        {
            var result = await _requestRemarksRepository.GetAllRemarks(requestid);
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }

        [HttpPatch("cancel/{requestid}/{user}")]
        public async Task<ActionResult> CancelRequest(long requestid, string user, [FromBody] string remarks)
        {
            if (requestid <= 0)
            {
                return BadRequest("RequestID is required and must be greater than zero.");
            }

            var result = await _requestRemarksRepository.InsertRemarkAndCancelRequestForPullOutReturnPickUp(requestid,user, remarks);
            if (!result)
            {
                return NotFound("Request not found or cancel failed.");
            }

            return Ok(new { message = "Request cancelled successfully." });
        }

        [HttpGet("image")]
        public async Task<ActionResult> GetRequestImage([FromQuery] string requestid, [FromQuery] string type)
        {
            if (string.IsNullOrWhiteSpace(requestid) || string.IsNullOrWhiteSpace(type))
            {
                return BadRequest("RequestID and type are required.");
            }
            var imageBytes = await _imagePathTypeRepository.GetRequestImage(requestid, type);
            if (imageBytes == null)
            {
                return NotFound();
            }
            return File(imageBytes, "image/png");
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
                return BadRequest("No image uploaded.");

            using var ms = new MemoryStream();
            await image.CopyToAsync(ms);
            byte[] imageBytes = ms.ToArray();

            var filePath = await _imagePathTypeRepository.UploadImageAsync(imageBytes, dto.RequestID!, dto.Type!);
            if (filePath == null)
            {
                return StatusCode(500, "Failed to upload image.");
            }

            return Ok();
        }
    }
}