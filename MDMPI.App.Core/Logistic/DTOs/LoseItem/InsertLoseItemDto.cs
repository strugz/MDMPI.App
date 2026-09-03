using System.Text.Json.Serialization;

namespace MDMPI.App.Core.Logistic.DTOs.LoseItem
{
    public class InsertLoseItemDto
    {
        [JsonPropertyName("Item Code")]
        public string? ItemCode { get; set; }

        [JsonPropertyName("Remarks")]
        public string? Remarks { get; set; }
    }
}
