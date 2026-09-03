using System.Text.Json.Serialization;

namespace MDMPI.App.Core.Logistic.DTOs.LoseItem
{
    public class FetchLoseItemDto
    {
        [JsonPropertyName("LoseItemID")]
        public long LoseItemID { get; set; }

        [JsonPropertyName("RequestID")]
        public string? RequestID { get; set; }

        [JsonPropertyName("Item Code")]
        public string? ItemCode { get; set; }

        [JsonPropertyName("Remarks")]
        public string? Remarks { get; set; }

        [JsonPropertyName("CreatedAt")]
        public string? CreatedAt { get; set; }
    }
}
