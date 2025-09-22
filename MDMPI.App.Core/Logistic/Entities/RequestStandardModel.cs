using MDMPI.App.Core.Common.Entities;
using MDMPI.App.Core.CommonOldEntities.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MDMPI.App.Core.Logistic.Entities
{
    public class RequestStandardModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long RequestID { get; set; }
        public string? RequestClientID { get; set; }
        public string? RequestShippingMethod { get; set; }
        public string? RequestDeliveryTerms { get; set; }
        public DateOnly? RequestDeliveryDate { get; set; } // [date]
        public string? RequestPreference { get; set; }
        public string? RequestStatus { get; set; }
        public string? RequestBy { get; set; }
        public string? RequestCreatedBy { get; set; }
        public string? RequestItemPreparedBy { get; set; }
        public string? RequestDeliveredBy { get; set; }
        public DateTime? RequestCreatedAt { get; set; } // [datetime]
        public DateTime? RequestItemPreparedAt { get; set; } // [datetime]
        public DateTime? RequestItemPreparedEndAt { get; set; } // [datetime]
        public DateTime? RequestDeliveredAt { get; set; } // [datetime]
        public DateTime? RequestDeliveredEndAt { get; set; } // [datetime]
        public string? LocationStartedAt { get; set; }
        public string? LocationEndAt { get; set; }
        public long? MobileID { get; set; }
        public string? RequestDriverHelper { get; set; }
        public string? Receiver { get; set; }
        public string? RequestTripTicketNumber { get; set; }

        /// <summary>
        /// Enriched to support multiple document references
        /// </summary>
        [ForeignKey("RequestID")]
        public List<DocumentReferenceModel>? DocumentReference { get; set; }
        [ForeignKey("RequestID")]
        public ImageModel? Image { get; set; }
        [ForeignKey("RequestID")]
        public SignatureModel? Signature  { get; set; }
        [ForeignKey("RequestID")]
        public RemarksModel? Remarks { get; set; }
        [ForeignKey("MobileID")]
        public MobileModel? Mobile { get; set; }
        [ForeignKey("RequestClientID")]
        public ACCMSTModel? Client { get; set; }
    }
}
