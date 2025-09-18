namespace MDMPI.App.Core.Logistic.DTOs.RequestPullOutReturnPickUp
{
    public class DisplayRequestPullOutReturnPickUpDto
    {
        public long RequestID { get; set; }
        public string? Client { get; set; }
        public string? ClientContactPerson { get; set; }
        public string? ClientAddress { get; set; }
        public string? Category { get; set; }
        public string? SlipNo { get; set; }
        public string? IRRFNumber { get; set; }
        public DateTime? IRRFDate { get; set; }
        public string? ReasonForReturn { get; set; }
        public string? DocumentReference { get; set; }
        public string? ReleasedBy { get; set; }
        public string? ItemCategory { get; set; }
        public DateTime? PullOutDate { get; set; }
        public DateTime? PullOutDateEndAt { get; set; }
        public string? RequestStatus { get; set; }
        public string? TripTicketNumber { get; set; }
        public string? Driver { get; set; }
        public string? Helper { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
