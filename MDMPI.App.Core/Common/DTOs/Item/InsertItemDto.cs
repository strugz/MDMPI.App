using System.Text.Json.Serialization;

namespace MDMPI.App.Core.Common.DTOs.Item
{
    public class InsertItemDto
    {
        [JsonPropertyName("Item Code")]
        public string? ItemCode { get; set; }

        [JsonPropertyName("Description")]
        public string? Description { get; set; }

        [JsonPropertyName("Qty")]
        public decimal? Qty { get; set; }

        [JsonPropertyName("Unit")]
        public string? Unit { get; set; } = null;

        [JsonPropertyName("batch")]
        public List<InsertItemBatchDto>? Batch { get; set; }
    }
}