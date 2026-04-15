using MDMPI.App.Core.Common.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDMPI.App.Core.Logistic.DTOs.RequestStandard
{
    public class UpdateRequestDto
    {
        public string? RequestID { get; set; }
        public string? RequestStatus { get; set; }
        public string? RequestItemPreparedBy { get; set; }
        public string? RequestDeliveredBy { get; set; }
        public string? RequestDriverHelper { get; set; }
        public long? MobileID { get; set; }
        public string? Receiver { get; set; }
        public string? RequestTripTicketNumber { get; set; }
        public string? RequestItemPreparedAt { get; set; }
        public string? RequestItemPreparedEndAt { get; set; }
        public string? RequestDeliveredAt { get; set; }
        public string? RequestDeliveredEndAt { get; set; }
        public string? LocationStartedAt { get; set; }
        public string? LocationEndAt { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
