using MDMPI.App.Core.Common.DTOs;
using MDMPI.App.Core.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MDMPI.App.Api.Controllers.Common
{
    [Route("api/[controller]")]
    [ApiController]
    public class GeminiController : ControllerBase
    {
        private readonly IGeminiService _geminiService;
        private readonly MDMPI.App.Core.Common.Services.GeminiSettings _geminiSettings;

        public GeminiController(IGeminiService geminiService, Microsoft.Extensions.Options.IOptions<MDMPI.App.Core.Common.Services.GeminiSettings> geminiOptions)
        {
            _geminiService = geminiService;
            _geminiSettings = geminiOptions?.Value ?? new MDMPI.App.Core.Common.Services.GeminiSettings();
        }

        /// <summary>
        /// Analyzes an uploaded image using Google Gemini AI.
        /// Accepts a JSON body with base64-encoded image data.
        /// </summary>
        [HttpPost("analyze")]
        public async Task<ActionResult<GeminiAnalyzeImageResponseDto>> AnalyzeImage([FromBody] GeminiAnalyzeImageRequestDto request)
        {
            if (string.IsNullOrEmpty(request.ImageBase64))
                return BadRequest("Image data is required.");

            var result = await _geminiService.AnalyzeImageAsync(request);

            if (!result.Success)
                return StatusCode(502, result);

            return Ok(result);
        }

        /// <summary>
        /// Analyzes an uploaded image file using Google Gemini AI.
        /// Accepts multipart/form-data with an image file and optional prompt.
        /// </summary>
        [HttpPost("analyze-file")]
        public async Task<ActionResult<GeminiAnalyzeImageResponseDto>> AnalyzeImageFile(IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
                return BadRequest("Image file is required.");

            using var memoryStream = new MemoryStream();
            await imageFile.CopyToAsync(memoryStream);
            var imageBytes = memoryStream.ToArray();

            var request = new GeminiAnalyzeImageRequestDto
            {
                ImageBase64 = Convert.ToBase64String(imageBytes),
                MimeType = imageFile.ContentType ?? "image/jpeg",
                Prompt = string.IsNullOrWhiteSpace(_geminiSettings.Prompt) ? null : _geminiSettings.Prompt
            };

            var result = await _geminiService.AnalyzeImageAsync(request);

            if (!result.Success)
                return StatusCode(502, result);

            return Ok(result);
        }
    }
}
