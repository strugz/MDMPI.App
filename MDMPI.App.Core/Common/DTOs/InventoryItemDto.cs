using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace MDMPI.App.Core.Common.DTOs
{
    public class InventoryItemDto
    {
        [JsonPropertyName("Item Code")]
        public string ItemCode { get; set; } = string.Empty;

        [JsonPropertyName("Description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("Qty")]
        public decimal Qty { get; set; }

        [JsonPropertyName("Unit")]
        public string Unit { get; set; } = string.Empty;
    }

    public static partial class InventoryItemDtoExtensions
    {
        /// <summary>
        /// Parses the raw Gemini content (may include triple-backtick code fences and an optional language tag)
        /// and returns a list of <see cref="InventoryItemDto"/> instances.
        /// </summary>
        public static List<InventoryItemDto> FromGeminiContent(string? content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return new List<InventoryItemDto>();

            try
            {
                // Remove triple-backtick fences and optional language tag (e.g. "```json\n" ... "```")
                content = Regex.Replace(content, "^```(?:\\w+)?\\s*", "", RegexOptions.Singleline);
                content = Regex.Replace(content, "\\s*```$", "", RegexOptions.Singleline);

                // If the content contains additional text, try to extract the JSON array by finding the first '[' and the last ']'
                var start = content.IndexOf('[');
                var end = content.LastIndexOf(']');
                if (start >= 0 && end > start)
                {
                    content = content.Substring(start, end - start + 1);
                }

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                options.Converters.Add(new StringToDecimalConverter());

                var items = JsonSerializer.Deserialize<List<InventoryItemDto>>(content, options);
                return items ?? new List<InventoryItemDto>();
            }
            catch
            {
                // If parsing fails, return an empty list. Caller can detect emptiness as needed.
                return new List<InventoryItemDto>();
            }
        }
    }

    /// <summary>
    /// Converter to handle numeric values provided as strings (e.g., "1.00") or numbers.
    /// </summary>
    public class StringToDecimalConverter : JsonConverter<decimal>
    {
        public override decimal Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Number && reader.TryGetDecimal(out var value))
            {
                return value;
            }

            if (reader.TokenType == JsonTokenType.String)
            {
                var s = reader.GetString();
                if (decimal.TryParse(s, out var result))
                    return result;
            }

            throw new JsonException($"Unable to convert token of type {reader.TokenType} to decimal.");
        }

        public override void Write(Utf8JsonWriter writer, decimal value, JsonSerializerOptions options)
        {
            // Write as number
            writer.WriteNumberValue(value);
        }
    }
}
