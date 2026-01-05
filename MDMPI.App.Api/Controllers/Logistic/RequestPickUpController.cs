using MDMPI.App.Core.Common.DTOs;
using MDMPI.App.Core.Logistic.DTOs.RequestPickUp;
using MDMPI.App.Core.Logistic.Interfaces;
using Microsoft.AspNetCore.Mvc;
using MDMPI.App.Api.Models;

namespace MDMPI.App.Api.Controllers.Logistic
{
    [Route("api/[controller]")]
    [ApiController]
    public class RequestPickUpController : ControllerBase
    {
        private readonly IRequestPickUpRepository _repository;
        private readonly IImagePathTypeRepository _imagePathTypeRepository;
        private readonly IRequestRemarksRepository _requestRemarksRepository;
        private readonly IMobileRepository _mobileRepository;

        public RequestPickUpController(IRequestPickUpRepository repository, IImagePathTypeRepository imagePathTypeRepository, IRequestRemarksRepository requestRemarksRepository, IMobileRepository mobileRepository)
        {
            _repository = repository;
            _imagePathTypeRepository = imagePathTypeRepository;
            _requestRemarksRepository = requestRemarksRepository;
            _mobileRepository = mobileRepository;
        }

        [HttpGet]
        public async Task<ActionResult> GetAll([FromQuery] RequestDateFilter dateFilter = RequestDateFilter.All, [FromQuery] RequestStatusFilter statusFilter = RequestStatusFilter.All, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var query = new RequestQueryDto
            {
                DateFilter = dateFilter,
                StatusFilter = statusFilter,
                Page = page,
                PageSize = pageSize
            };

            var result = await _repository.GetAllAsync(query);
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult> Insert([FromBody] InsertRequestPickUpDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var inserted = await _repository.InsertAsync(dto);
            if (inserted is null)
            {
                return BadRequest("Insert failed.");
            }

            return CreatedAtAction(nameof(GetAll), new { id = inserted.RequestID }, inserted);
        }

        [HttpPatch]
        public async Task<ActionResult> Update([FromBody] UpdateRequestPickUpDto dto)
        {
            if (dto.RequestID is null || dto.RequestID <= 0)
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

            var result = await _requestRemarksRepository.InsertRemarkAndCancelRequestForPickUp(requestid, user, remarks);
            if (!result)
            {
                return NotFound("Request not found or cancel failed.");
            }

            return Ok(new { message = "Request cancelled successfully." });
        }

        [HttpGet("mobile")]
        public async Task<ActionResult> GetMobile()
        {
            var result = await _mobileRepository.GetAllMobilesAsync();
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
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

            if (string.IsNullOrWhiteSpace(dto.Type) || (dto.Type != "Signature" && dto.Type != "Proof"))
            {
                return BadRequest("Type must be 'Signature' or 'Proof'.");
            }

            var filePath = await _imagePathTypeRepository.UploadImageAsync(imageBytes, dto.RequestID!, dto.Type!);
            if (filePath == null)
            {
                return StatusCode(500, "Failed to upload image.");
            }

            return Ok();
        }
    }
}
