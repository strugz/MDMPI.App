using System;
using System.Text.Json.Serialization;

namespace MDMPI.App.Core.Logistic.DTOs.RequestBackload
{
    public class InsertRequestBackloadDto
    {
        [JsonPropertyName("RequestID")]
        public long? RequestID { get; set; }

        [JsonPropertyName("Remarks")]
        public string? Remarks { get; set; }

        [JsonPropertyName("DateReported")]
        public DateTime? DateReported { get; set; }
    }
}
