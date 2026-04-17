using MDMPI.App.Api.Models;
using MDMPI.App.Core.Common.DTOs;
using MDMPI.App.Core.Common.Interfaces;
using MDMPI.App.Core.Logistic.DTOs.RequestAirSea;
using MDMPI.App.Core.Logistic.Interfaces;
using MDMPI.App.Core.Logistic.Services;
using Microsoft.AspNetCore.Mvc;

namespace MDMPI.App.Api.Controllers.Logistic
{
    [Route("api/[controller]")]
    [ApiController]
    public class RequestAirSeaController : ControllerBase
    {
        private readonly IRequestAirSeaService _airSeaService;
        private readonly IImageService _imageService;
        private readonly IRemarksService _remarksService;
        private readonly IMobileService _mobileService;

        public RequestAirSeaController(IRequestAirSeaService airSeaService, IImageService imageService, IRemarksService remarksService, IMobileService mobileService)
        {
            _airSeaService = airSeaService;
            _imageService = imageService;
            _remarksService = remarksService;
            _mobileService = mobileService;
        }

        [HttpGet]
        public async Task<ActionResult> GetAll([FromQuery] RequestDateFilter dateFilter = RequestDateFilter.All,[FromQuery] RequestStatusFilter statusFilter = RequestStatusFilter.All,[FromQuery] int page = 1,[FromQuery] int pageSize = 20)
        {
            var query = new RequestQueryDto
            {
                DateFilter = dateFilter,
                StatusFilter = statusFilter,
                Page = page,
                PageSize = pageSize
            };

            var result = await _airSeaService.GetAllAsync(query);
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult> Insert([FromBody] InsertRequestAirSeaDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var inserted = await _airSeaService.InsertAsync(dto);
            if (inserted is null)
            {
                return BadRequest("Insert failed.");
            }

            return CreatedAtAction(nameof(GetAll), new { id = inserted.RequestID }, inserted);
        }

        [HttpPatch]
        public async Task<ActionResult> Update([FromBody] UpdateRequestAirSeaDto dto)
        {
            if (dto.RequestID <= 0)
            {
                return BadRequest("RequestID is required.");
            }

            var success = await _airSeaService.UpdateAsync(dto);
            if (!success)
            {
                return NotFound("Request not found or update failed.");
            }

            return Ok("Request updated successfully.");
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

            var result = await _remarksService.CancelAirSeaAsync(requestid, user, remarks);
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

            var result = await _airSeaService.GetHistoryAsync(requestid);
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
            var imageBytes = ms.ToArray();

            if (string.IsNullOrWhiteSpace(dto.Type) || (dto.Type != "Signature" && dto.Type != "Proof"))
            {
                return BadRequest("Type must be 'Signature' or 'Proof'.");
            }

            var filePath = await _imageService.UploadImageAsync(imageBytes, dto.RequestID!, dto.Type!);
            if (filePath == null)
            {
                return StatusCode(500, "Failed to upload image.");
            }

            return Ok();
        }
    }
}
