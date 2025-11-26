using MDMPI.App.Core.Common.Entities;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MDMPI.App.Core.Logistic.DTOs.RequestPullOutReturnPickUp
{
    public class UpdateRequestPullOutReturnPickUpDto
    {
        public long? RequestID { get; set; }
        public long? MobileID { get; set; }
        public string? ClientContactPerson { get; set; }
        public string? ReasonForReturn { get; set; }
        public string? ReleasedBy { get; set; }
        public DateTime? PullOutDateStartAt { get; set; }
        public DateTime? PullOutDateEndAt { get; set; }
        public string? RequestStatus { get; set; }
        public string? TripTicketNumber { get; set; }
        public string? Driver { get; set; }
        public string? Helper { get; set; }
        public string? ReceivedBy { get; set; }
    }
}
