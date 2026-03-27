using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MDMPI.App.Core.Common.DTOs.Item
{
    public class FetchItemDto
    {
        // Expose both identifiers to support current DB usage and callers that prefer RequestID.
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
        [JsonPropertyName("Batch Count")]
        public int BatchCount { get; set; }

        [JsonPropertyName("Batch")]
        public List<FetchItemBatchDto>? Batch { get; set; }
    }
}