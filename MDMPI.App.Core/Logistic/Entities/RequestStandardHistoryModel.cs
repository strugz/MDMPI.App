using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MDMPI.App.Core.Logistic.Entities
{
    public class RequestStandardHistoryModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long? HistoryID { get; set; }

        public string? ActionType { get; set; }

        public DateTime? ChangedAt { get; set; }

        public string? ChangedBy { get; set; }
        public long? RequestID { get; set; }

        public long? ItemCategoryID { get; set; }

        public long? FormCategoryID { get; set; }

        public string? RequestClientID { get; set; }

        public string? RequestShippingMethod { get; set; }

        public string? RequestDeliveryTerms { get; set; }

        public DateTime? RequestDeliveryDate { get; set; }

        public string? RequestPreference { get; set; }
        public string? RequestStatus { get; set; }

        public string? RequestBy { get; set; }

        public string? RequestCreatedBy { get; set; }
        public string? RequestItemPreparedBy { get; set; }

        public string? RequestDeliveredBy { get; set; }

        public DateTime? RequestCreatedAt { get; set; }

        public DateTime? RequestItemPreparedAt { get; set; }

        public DateTime? RequestItemPreparedEndAt { get; set; }

        public DateTime? RequestDeliveredAt { get; set; }

        public DateTime? RequestDeliveredEndAt { get; set; }

        public string? LocationStartedAt { get; set; }

        public string? LocationEndAt { get; set; }

        public long? MobileID { get; set; }

        public string? RequestDriverHelper { get; set; }

        public string? Receiver { get; set; }

        public string? RequestTripTicketNumber { get; set; }
    }
}
