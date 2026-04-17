using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MDMPI.App.Core.Logistic.Entities
{
    public class RequestAirSeaHistoryModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long? HistoryID { get; set; }

        public string? ActionType { get; set; }

        public DateTime? ChangedAt { get; set; }

        public string? ChangedBy { get; set; }

        public long? RequestID { get; set; }

        [MaxLength(100)]
        public string? ClientID { get; set; }

        public long? ItemCategoryID { get; set; }

        [MaxLength(100)]
        public string? ReceivedBy { get; set; }

        [MaxLength(100)]
        public string? WaybillNumber { get; set; }

        [MaxLength(100)]
        public string? TripTicketNumber { get; set; }

        [MaxLength(100)]
        public string? Driver { get; set; }

        [MaxLength(100)]
        public string? Helper { get; set; }

        public long? MobileID { get; set; }

        public DateTime? DatePickUp { get; set; }

        public DateTime? ItemPreparedAt { get; set; }

        public DateTime? ItemPreparedEndAt { get; set; }

        public DateTime? DispatchedAt { get; set; }

        public DateTime? DropOffAt { get; set; }

        [MaxLength(4)]
        public string? PreparedBy { get; set; }

        [MaxLength(15)]
        public string? Status { get; set; }

        [MaxLength(255)]
        public string? Remarks { get; set; }

        [MaxLength(100)]
        public string? CreatedBy { get; set; }

        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}
