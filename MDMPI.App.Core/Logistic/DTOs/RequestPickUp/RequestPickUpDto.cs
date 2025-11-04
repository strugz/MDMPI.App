using System;
using System.Collections.Generic;

namespace MDMPI.App.Core.Logistic.DTOs.RequestPickUp
{
    public class RequestPickUpDto
    {
        public long RequestID { get; set; }
        public string? ClientID { get; set; }
        public long? ItemCategoryID { get; set; }
        public List<string>? DocumentReference { get; set; }
        public string? PreparedBy { get; set; }
        public DateTime? ItemPreparedAt { get; set; }
        public DateTime? ItemPreparedEndAt { get; set; }
        public DateTime? DatePickUp { get; set; }
        public string? Remarks { get; set; }
        public string? Status { get; set; }
        public string? ReleasedBy { get; set; }
        public string? ReceivedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
