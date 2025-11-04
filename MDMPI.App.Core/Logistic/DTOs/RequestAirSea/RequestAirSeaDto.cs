using MDMPI.App.Core.Common.DTOs;
using MDMPI.App.Core.CommonOldEntities.DTOs;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MDMPI.App.Core.Logistic.DTOs.RequestAirSea
{
    public class RequestAirSeaDto
    {
        public long RequestID { get; set; }
        public long? ItemCategoryID { get; set; }
        public string? ClientID { get; set; }
        public List<string>? DocumentReference { get; set; }
        public long? MobileID { get; set; }
        public string? RiderName { get; set; }
        public DateTime? DatePickUp { get; set; }
        public DateTime? ItemPreparedAt { get; set; }
        public DateTime? ItemPreparedEndAt { get; set; }
        public string? PreparedBy { get; set; }
        public string? Status { get; set; }
        public string? Remarks { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        [JsonPropertyName("Signature")]
        public SignatureDto? Signature { get; set; }
        [JsonPropertyName("Client")]
        public ACCMSTDto? Client { get; set; }
    }
}
