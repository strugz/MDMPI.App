using System.Text.Json.Serialization;

namespace MDMPI.App.Core.Common.DTOs
{
    public class FetchItemBatchDto
    {
        [JsonPropertyName("RequestItemBatchID")]
        public long? RequestItemBatchID { get; set; }

        [JsonPropertyName("RequestItemID")]
        public long? RequestItemID { get; set; }

        [JsonPropertyName("Batch/Serial #")]
        public string? BatchSerial { get; set; }

        [JsonPropertyName("Batch Quantity")]
        public decimal? BatchQuantity { get; set; }

        [JsonPropertyName("Expiry Date")]
        public string? ExpiryDate { get; set; }
    }
}