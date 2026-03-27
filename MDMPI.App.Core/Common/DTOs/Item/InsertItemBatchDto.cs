using System.Text.Json.Serialization;

namespace MDMPI.App.Core.Common.DTOs.Item
{
    public class InsertItemBatchDto
    {
        [JsonPropertyName("Batch/Serial #")]
        public string? BatchSerial { get; set; }

        [JsonPropertyName("Batch Quantity")]
        public decimal? BatchQuantity { get; set; }

        [JsonPropertyName("Expiry Date")]
        public string? ExpiryDate { get; set; }
    }
}