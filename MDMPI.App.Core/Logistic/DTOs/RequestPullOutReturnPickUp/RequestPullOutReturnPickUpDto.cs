using MDMPI.App.Core.CommonOldEntities.DTOs;
using System;
using System.Text.Json.Serialization;

namespace MDMPI.App.Core.Logistic.DTOs.RequestPullOutReturnPickUp
{
    public class RequestPullOutReturnPickUpDto
    {
        public long? RequestID { get; set; }
        public string? ClientID { get; set; }
        public string? ClientContactPerson { get; set; }
        public long? FormCategoryID { get; set; }
        public long? ItemCategoryID { get; set; }
        public string? SlipNo { get; set; }
        public string? IRRFNumber { get; set; }
        public DateTime? IRRFDate { get; set; }
        public string? ReasonForReturn { get; set; }
        public string? ReleasedBy { get; set; }
        public DateTime? PullOutDate { get; set; }
        public DateTime? PullOutDateStartAt { get; set; }
        public DateTime? PullOutDateEndAt { get; set; }
        public string? RequestStatus { get; set; }
        public string? TripTicketNumber { get; set; }
        public string? Driver { get; set; }
        public string? Helper { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        [JsonPropertyName("Client")]
        public ACCMSTDto? Client { get; set; }
        [JsonPropertyName("DocumentReference")]
        public List<string>? DocumentReference { get; set; }
    }
}
