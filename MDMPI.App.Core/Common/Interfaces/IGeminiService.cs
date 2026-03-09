using MDMPI.App.Core.Common.DTOs;

namespace MDMPI.App.Core.Common.Interfaces
{
    public interface IGeminiService
    {
        /// <summary>
        /// Sends an image to Google Gemini AI and returns structured analysis results.
        /// </summary>
        Task<GeminiAnalyzeImageResponseDto> AnalyzeImageAsync(GeminiAnalyzeImageRequestDto request);
    }
}
