using MDMPI.App.Core.Common.DTOs;
using MDMPI.App.Core.Common.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MDMPI.App.Core.Logistic.DTOs.RequestStandard
{
    public class RequestStandardDto
    {
        [JsonPropertyName("ID")]
        public string? ID { get; set; }
        [JsonPropertyName("ClientID")]
        public string? ClientID { get; set; }
        [JsonPropertyName("FormCategoryID")]
        public long? FormCategoryID { get; set; }
        [JsonPropertyName("ItemCategoryID")]
        public long? ItemCategoryID { get; set; }
        [JsonPropertyName("ShippingMethod")]
        public string? ShippingMethod { get; set; }
        [JsonPropertyName("DeliveryTerms")]
        public string? DeliveryTerms { get; set; }
        [JsonPropertyName("DeliveryDate")]
        public string? DeliveryDate { get; set; }
        [JsonPropertyName("Preference")]
        public string? Preference { get; set; }
        [JsonPropertyName("Status")]
        public string? Status { get; set; }
        [JsonPropertyName("RequestBy")]
        public string? RequestBy { get; set; }
        [JsonPropertyName("CreatedBy")]
        public string? CreatedBy { get; set; }
        [JsonPropertyName("CreatedAt")]
        public string? CreatedAt { get; set; }
        [JsonPropertyName("ItemPreparedBy")]
        public string? ItemPreparedBy { get; set; }
        [JsonPropertyName("DeliveredBy")]
        public string? DeliveredBy { get; set; }
        [JsonPropertyName("ItemPreparedAt")]
        public string? ItemPreparedAt { get; set; }
        [JsonPropertyName("ItemPreparedEndAt")]
        public string? ItemPreparedEndAt { get; set; }
        [JsonPropertyName("DeliveredAt")]
        public string? DeliveredAt { get; set; }
        [JsonPropertyName("DeliveredEndAt")]
        public string? DeliveredEndAt { get; set; }
        [JsonPropertyName("MobileID")]
        public long? MobileID { get; set; }
        [JsonPropertyName("MobileName")]
        public string? MobileName { get; set; }
        [JsonPropertyName("Helper")]
        public string? Helper { get; set; }
        [JsonPropertyName("Receiver")]
        public string? Receiver { get; set; }
        [JsonPropertyName("TripTicketNumber")]
        public string? TripTicketNumber { get; set; }
        [JsonPropertyName("LocationStartedAt")]
        public string? LocationStartedAt { get; set; }
        [JsonPropertyName("LocationEndAt")]
        public string? LocationEndAt { get; set; }
        [JsonPropertyName("UpdatedBy")]
        public string? UpdatedBy  { get; set; }

        // Enriched
        [JsonPropertyName("Client")]
        public ACCMSTDto? Client { get; set; }
        [JsonPropertyName("DocumentReference")]
        public List<string>? DocumentReference { get; set; }
        [JsonPropertyName("CancelRemarks")]
        public RemarksDto? CancelRemarks { get; set; }
        [JsonPropertyName("Image")]
        public ImageDto? Image { get; set; }
        [JsonPropertyName("Signature")]
        public SignatureDto? Signature { get; set; }

    }
}
