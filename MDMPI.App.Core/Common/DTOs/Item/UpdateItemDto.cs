using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MDMPI.App.Core.Common.DTOs.Item
{
    public class UpdateItemDto
    {
        // Keep both IDs for backward compatibility; prefer RequestID per project guideline.
        [JsonPropertyName("RequestItemID")]
        public long? RequestItemID { get; set; }

        [JsonPropertyName("RequestID")]
        public string? RequestID { get; set; }

        [JsonPropertyName("Item Code")]
        public string? ItemCode { get; set; }

        [JsonPropertyName("Description")]
        public string? Description { get; set; }

        [JsonPropertyName("Qty")]
        public decimal? Qty { get; set; }

        [JsonPropertyName("Unit")]
        public string? Unit { get; set; }

        [JsonPropertyName("batch")]
        public List<UpdateItemBatchDto>? Batch { get; set; }
    }
}