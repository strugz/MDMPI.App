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
        private readonly IRequestService _requestService;
        private readonly IMobileService _mobileService;
        private readonly IRemarksService _remarksService;
        private readonly IImageService _imageService;

        public RequestController(
            IRequestService requestService,
            IMobileService mobileService,
            IRemarksService remarksService,
            IImageService imageService)
        {
            _requestService = requestService;
            _mobileService = mobileService;
            _remarksService = remarksService;
            _imageService = imageService;
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

            var result = await _requestService.GetAllRequestsAsync(query);

            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }

        [HttpGet("cancel/{requestid}")]
        public async Task<ActionResult> GetCancelledRemarks(long requestid)
        {
            var result = await _remarksService.GetAllRemarks(requestid);
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

            var result = await _remarksService.CancelStandardDeliveryAsync(requestid, user, remarks);
            if (!result)
            {
                return NotFound("Request not found or cancel failed.");
            }

            return Ok(new { message = "Request cancelled successfully." });
        }

        [HttpGet("mobile")]
        public async Task<ActionResult> GetMobile()
        {
            var result = await _mobileService.GetAllMobilesAsync();
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult> PostRequest([FromBody] InsertRequestDto value)
        {
            var inserted = await _requestService.CreateRequestWithItemsAsync(value);

            if (inserted is null)
            {
                return BadRequest("Insert failed.");
            }

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

            var result = await _requestService.UpdateRequestAsync(value);
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
            var imageBytes = await _imageService.GetRequestImageAsync(requestid, type);

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

            var result = await _requestService.GetRequestHistoryAsync(requestid);
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

            var filePath = await _imageService.UploadImageAsync(imageBytes, dto.RequestID!, dto.Type!);
            if (filePath == null)
            {
                return StatusCode(500, "Failed to upload image.");
            }

            return Ok();
        }
    }
}
