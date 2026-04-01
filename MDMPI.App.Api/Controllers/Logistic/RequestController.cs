using MDMPI.App.Api.Models;
using MDMPI.App.Core.Common.DTOs;
using MDMPI.App.Core.Common.Interfaces;
using MDMPI.App.Core.Logistic.DTOs.RequestStandard;
using MDMPI.App.Core.Logistic.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Numerics;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MDMPI.App.Api.Controllers.Logistic
{
    [Route("api/[controller]")]
    [ApiController]
    public class RequestController : ControllerBase
    {
        private readonly IRequestRepository _requestRepository;
        private readonly IMobileRepository _mobileRepository;
        private readonly IRequestRemarksRepository _requestRemarksRepository;
        private readonly IImagePathTypeRepository _imagePathTypeRepository;
        private readonly IItemRepository _itemRepository;

        public RequestController(
            IRequestRepository requestRepository,
            IMobileRepository mobileRepository,
            IRequestRemarksRepository requestRemarksRepository,
            IImagePathTypeRepository imagePathTypeRepository,
            IItemRepository itemRepository)
        {
            _requestRepository = requestRepository;
            _mobileRepository = mobileRepository;
            _requestRemarksRepository = requestRemarksRepository;
            _imagePathTypeRepository = imagePathTypeRepository;
            _itemRepository = itemRepository;
        }

        [HttpGet]
        public async Task<ActionResult> GetRequestAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] RequestDateFilter dateFilter = RequestDateFilter.All, [FromQuery] RequestStatusFilter statusFilter = RequestStatusFilter.All)
        {
            var query = new RequestQueryDto
            {
                Page = page,
                PageSize = pageSize,
                StatusFilter = statusFilter,
                DateFilter = dateFilter
            };

            var result = await _requestRepository.GetAllRequestsAsync(query);

            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
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

            var result = await _requestRemarksRepository.InsertRemarkAndCancelRequestForStandardDeliveryAsync(requestid, user, remarks);
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

        [HttpPost]
        public async Task<ActionResult> PostRequest([FromBody] InsertRequestDto value)
        {
            var inserted = await _requestRepository.InsertRequest(value);

            if (inserted is null)
            {
                return BadRequest("Insert failed.");
            }

            await _itemRepository.InsertItemsAsync(long.Parse(inserted.ID!), value.Items!);


            return CreatedAtAction(nameof(GetRequestAll), new { id = inserted.ID }, inserted);
        }

        [HttpPatch]
        public async Task<ActionResult> UpdateRequest([FromBody] UpdateRequestDto value)
        {
            // Basic validation
            if (long.Parse(value.RequestID!) <= 0)
            {
                return BadRequest("RequestID is required and must be greater than zero.");
            }

            var result = await _requestRepository.UpdateRequest(value);
            if (!result)
            {
                return NotFound("Request not found or update failed.");
            }
            return Ok("Request updated successfully.");
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

        [HttpGet("history/{requestid}")]
        public async Task<ActionResult> GetRequestHistory(long requestid)
        {
            if (requestid <= 0)
            {
                return BadRequest("RequestID is required and must be greater than zero.");
            }

            var result = await _requestRepository.GetAllRequestHistory(requestid);
            if (result == null || result.Count == 0)
            {
                return NotFound();
            }
            return Ok(result);
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
