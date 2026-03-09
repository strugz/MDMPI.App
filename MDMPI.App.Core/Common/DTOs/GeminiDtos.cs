using System.Collections.Generic;

namespace MDMPI.App.Core.Common.DTOs
{
    /// <summary>
    /// Request DTO for analyzing an image with Gemini AI.
    /// </summary>
    public class GeminiAnalyzeImageRequestDto
    {
        /// <summary>
        /// Base64-encoded image data.
        /// </summary>
        public string ImageBase64 { get; set; } = string.Empty;

        /// <summary>
        /// MIME type of the image (e.g., "image/jpeg", "image/png").
        /// </summary>
        public string MimeType { get; set; } = "image/jpeg";

        /// <summary>
        /// Optional prompt/instruction to guide the AI analysis.
        /// </summary>
        public string? Prompt { get; set; }
    }

    /// <summary>
    /// Response DTO containing structured AI analysis results.
    /// </summary>
    public class GeminiAnalyzeImageResponseDto
    {
        public bool Success { get; set; }
        public string? Content { get; set; }
        public string? Error { get; set; }
        public List<InventoryItemDto>? InventoryItems { get; set; }
    }
}
