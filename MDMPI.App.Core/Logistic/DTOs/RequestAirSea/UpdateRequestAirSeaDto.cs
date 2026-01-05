using MDMPI.App.Core.Common.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace MDMPI.App.Core.Logistic.DTOs.RequestAirSea
{
    public class UpdateRequestAirSeaDto
    {
        public long RequestID { get; set; }
        public long? MobileID { get; set; }
        public string? ReceivedBy { get; set; }
        public string? WaybillNumber { get; set; }
        public string? TripTicketNumber { get; set; }
        public string? Driver { get; set; }
        public string? Helper { get; set; }
        public DateTime? ItemPreparedAt { get; set; }
        public DateTime? ItemPreparedEndAt { get; set; }
        public DateTime? DispatchedAt { get; set; }
        public DateTime? DropOffAt { get; set; }
        public string? PreparedBy { get; set; }
        public string? Status { get; set; }
        public string? Remarks { get; set; }
    }
}
