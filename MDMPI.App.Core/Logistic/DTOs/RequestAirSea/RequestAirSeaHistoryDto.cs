using System;

namespace MDMPI.App.Core.Logistic.DTOs.RequestAirSea
{
    public class RequestAirSeaHistoryDto
    {
        public long? HistoryID { get; set; }

        public string? ActionType { get; set; }

        public DateTime? ChangedAt { get; set; }

        public string? ChangedBy { get; set; }

        public long? RequestID { get; set; }

        public string? ClientID { get; set; }

        public long? ItemCategoryID { get; set; }

        public string? ReceivedBy { get; set; }

        public string? WaybillNumber { get; set; }

        public string? TripTicketNumber { get; set; }

        public string? Driver { get; set; }

        public string? Helper { get; set; }

        public long? MobileID { get; set; }

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

    }
}
