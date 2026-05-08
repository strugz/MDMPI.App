using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MDMPI.App.Core.Logistic.Entities
{
    public class RequestPullOutReturnPickUpHistoryModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long HistoryID { get; set; }

        public string? ActionType { get; set; }
        public DateTime? ChangedAt { get; set; }
        public string? ChangedBy { get; set; }
        public long RequestID { get; set; }
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
        public string? ReceivedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? RequestedBy { get; set; }
        public long? MobileID { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
