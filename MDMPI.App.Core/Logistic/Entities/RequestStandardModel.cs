using MDMPI.App.Core.Common.Entities;
using MDMPI.App.Core.CommonOldEntities.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MDMPI.App.Core.Logistic.Entities
{
    public class RequestStandardModel
    {
        [Key]
        public long RequestID { get; set; }
        public string? RequestClientID { get; set; }
        public string? RequestShippingMethod { get; set; }
        public string? RequestDeliveryTerms { get; set; }
        public string? RequestDeliveryDate { get; set; }
        public string? RequestPreference { get; set; }
        public string? RequestStatus { get; set; }
        public string? RequestBy { get; set; }
        public string? RequestCreatedBy { get; set; }
        public string? RequestItemPreparedBy { get; set; }
        public string? RequestDeliveredBy { get; set; }
        public string? RequestItemPreparedAt { get; set; }
        public string? RequestItemPreparedEndAt { get; set; }
        public string? RequestDeliveredAt { get; set; }
        public string? RequestDeliveredEndAt { get; set; }
        public string? LocationStartedAt { get; set; }
        public string? LocationEndAt { get; set; }

        public long? MobileID { get; set; } = 0;
        public string? RequestDriverHelper { get; set; }
        public string? Receiver { get; set; }
        public string? RequestTripTicketNumber { get; set; }

        public List<DocumentReferenceModel>? DocumentReference { get; set; }
        public ImageModel? Image { get; set; }
        public SignatureModel? Signature  { get; set; }
        public RemarksModel? Remarks { get; set; }
        public MobileModel? Mobile { get; set; }
        [ForeignKey("RequestClientID")]
        public ACCMSTModel? Client { get; set; }
    }
}
