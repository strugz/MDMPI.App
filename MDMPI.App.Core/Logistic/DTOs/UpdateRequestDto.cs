using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDMPI.App.Core.Logistic.DTOs
{
    public class UpdateRequestDto
    {
        public int RequestID { get; set; }
        public string? RequestShippingMethod { get; set; }
        public string? RequestDeliveryTerms { get; set; }
        public string? RequestDeliveryDate { get; set; }
        public string? RequestPreference { get; set; }
        public string? RequestStatus { get; set; }
        public string? RequestItemPreparedBy { get; set; }
        public string? RequestDeliveredBy { get; set; }
        public string? RequestDriverHelper { get; set; }
        public string? Receiver { get; set; }
        public string? RequestTripTicketNumber { get; set; }
        public string? RequestItemPreparedAt { get; set; }
        public string? RequestItemPreparedEndAt { get; set; }
        public string? RequestDeliveredAt { get; set; }
        public string? RequestDeliveredEndAt { get; set; }
        public string? LocationStartedAt { get; set; } // Coordinates as string
        public string? LocationEndAt { get; set; } // Coordinates as string
    }
}
