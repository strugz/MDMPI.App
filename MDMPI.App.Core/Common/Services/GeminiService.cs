using System.Text;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using MDMPI.App.Core.Common.DTOs;
using MDMPI.App.Core.Common.Interfaces;
using Microsoft.Extensions.Options;

namespace MDMPI.App.Core.Common.Services
{
    public class GeminiService : IGeminiService
    {
        private readonly HttpClient _httpClient;
        private readonly GeminiSettings _settings;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public GeminiService(HttpClient httpClient, IOptions<GeminiSettings> settings)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
        }

        public async Task<GeminiAnalyzeImageResponseDto> AnalyzeImageAsync(GeminiAnalyzeImageRequestDto request)
        {
            try
            {
                var prompt = request.Prompt ?? "Analyze this image and return the results as structured JSON.";

                var requestBody = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new object[]
                            {
                                new
                                {
                                    inlineData = new
                                    {
                                        mimeType = request.MimeType,
                                        data = request.ImageBase64
                                    }
                                },
                                new
                                {
                                    text = prompt
                                }
                            }
                        }
                    }
                };

                var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_settings.Model}:generateContent?key={_settings.ApiKey}";

                var jsonContent = JsonSerializer.Serialize(requestBody, JsonOptions);
                var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(url, httpContent);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return new GeminiAnalyzeImageResponseDto
                    {
                        Success = false,
                        Error = $"Gemini API error ({response.StatusCode}): {responseBody}"
                    };
                }

                // Parse the Gemini response to extract the text content
                var geminiResponse = JsonSerializer.Deserialize<GeminiApiResponse>(responseBody, JsonOptions);
                var textContent = geminiResponse?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;

                var inventoryItems = InventoryItemDtoExtensions.FromGeminiContent(textContent);

                return new GeminiAnalyzeImageResponseDto
                {
                    Success = true,
                    Content = textContent,
                    InventoryItems = inventoryItems
                };
            }
            catch (Exception ex)
            {
                return new GeminiAnalyzeImageResponseDto
                {
                    Success = false,
                    Error = $"Failed to analyze image: {ex.Message}"
                };
            }
        }

        #region Gemini API Response Models

        private class GeminiApiResponse
        {
            public List<GeminiCandidate>? Candidates { get; set; }
        }

        private class GeminiCandidate
        {
            public GeminiContent? Content { get; set; }
        }

        private class GeminiContent
        {
            public List<GeminiPart>? Parts { get; set; }
        }

        private class GeminiPart
        {
            public string? Text { get; set; }
        }

        #endregion
    }
}
