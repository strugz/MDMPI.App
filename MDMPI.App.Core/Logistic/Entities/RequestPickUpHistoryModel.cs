using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MDMPI.App.Core.Logistic.Entities
{
    public class RequestPickUpHistoryModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long HistoryID { get; set; }

        public string? ActionType { get; set; }
        public DateTime? ChangedAt { get; set; }
        public string? ChangedBy { get; set; }
        public long RequestID { get; set; }
        public string? ClientID { get; set; }
        public long? ItemCategoryID { get; set; }
        public string? PreparedBy { get; set; }
        public DateTime? ItemPreparedAt { get; set; }
        public DateTime? ItemPreparedEndAt { get; set; }
        public DateTime? DatePickUp { get; set; }
        public string? Remarks { get; set; }
        public string? Status { get; set; }
        public string? ReleasedBy { get; set; }
        public string? ReceivedBy { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
