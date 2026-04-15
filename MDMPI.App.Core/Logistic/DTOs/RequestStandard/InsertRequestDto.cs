using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using MDMPI.App.Core.Common.DTOs.Item;

namespace MDMPI.App.Core.Logistic.DTOs.RequestStandard
{
    public class InsertRequestDto
    {
        [JsonPropertyName("requestClientID")]
        public string? RequestClientID { get; set; }

        [JsonPropertyName("formCategoryID")]
        public long? FormCategoryID { get; set; }

        [JsonPropertyName("itemCategoryID")]
        public long? ItemCategoryID { get; set; }

        [JsonPropertyName("requestShippingMethod")]
        public string? RequestShippingMethod { get; set; } = string.Empty;

        [JsonPropertyName("requestDeliveryTerms")]
        public string? RequestDeliveryTerms { get; set; } = string.Empty;

        [JsonPropertyName("requestDeliveryDate")]
        public string? RequestDeliveryDate { get; set; }

        [JsonPropertyName("requestPreference")]
        public string? RequestPreference { get; set; } = string.Empty;

        [JsonPropertyName("requestStatus")]
        public string? RequestStatus { get; set; } = string.Empty;

        [JsonPropertyName("requestBy")]
        public string? RequestBy { get; set; } = string.Empty;

        [JsonPropertyName("requestCreatedBy")]
        public string? RequestCreatedBy { get; set; } = string.Empty;

        [JsonPropertyName("documentReference")]
        public List<string>? DocumentReference { get; set; }

        [JsonPropertyName("items")]
        public List<InsertItemDto>? Items { get; set; }
        [JsonPropertyName("UpdatedBy")]
        public string? UpdatedBy { get; set; }
    }
}
