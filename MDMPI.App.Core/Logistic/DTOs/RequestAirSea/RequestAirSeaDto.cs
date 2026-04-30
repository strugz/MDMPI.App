using MDMPI.App.Core.Common.DTOs;
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
        public string? ReceivedBy { get; set; }
        public string? WaybillNumber { get; set; }
        public string? TripTicketNumber { get; set; }
        public string? Driver { get; set; }
        public string? Helper { get; set; }
        public DateTime? DatePickUp { get; set; }
        public DateTime? ItemPreparedAt { get; set; }
        public DateTime? ItemPreparedEndAt { get; set; }
        public DateTime? DispatchedAt { get; set; }
        public DateTime? DropOffAt { get; set; }
        public string? PreparedBy { get; set; }
        public string? Status { get; set; }
        public string? Remarks { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? ProvincialPickUpBy { get; set; }
        public DateTime? ProvincialPickUpAt { get; set; }
        public DateTime? ProvincialInTransitAt { get; set; }
        public string? ProvincialInTransitLocation { get; set; }
        public string? ProvincialReceiverName { get; set; }
        public DateTime? ProvincialDeliveredEndAt { get; set; }
        public string? ProvincialDeliveredLocation { get; set; }
        [JsonPropertyName("Client")]
        public ACCMSTDto? Client { get; set; }
    }
}
