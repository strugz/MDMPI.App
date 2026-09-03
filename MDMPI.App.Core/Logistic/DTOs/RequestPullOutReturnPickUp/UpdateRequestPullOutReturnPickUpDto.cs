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

        /// <summary>
        /// When true, clears PullOutDateStartAt (used when a courier pauses an
        /// in-transit pull out back to For Pull Out, so the next departure
        /// stamps a fresh start time). Takes precedence over PullOutDateStartAt.
        /// </summary>
        public bool? ClearPullOutDateStartAt { get; set; }

        public DateTime? PullOutDateEndAt { get; set; }
        public string? RequestStatus { get; set; }
        public string? TripTicketNumber { get; set; }
        public string? Driver { get; set; }
        public string? Helper { get; set; }
        public string? ReceivedBy { get; set; }
    }
}
