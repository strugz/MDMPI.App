using MDMPI.App.Core.Common.Entities;
using MDMPI.App.Core.Common.Entities;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MDMPI.App.Core.Logistic.Entities
{
    public class RequestPullOutReturnPickUpModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long RequestID { get; set; }
        public string? ClientID { get; set; }
        public string? ClientContactPerson { get; set; }
        public long? FormCategoryID { get; set; }
        public long? ItemCategoryID { get; set; }
        public string? SlipNo { get; set; }
        public string? IRRFNumber { get; set; }
        public DateOnly? IRRFDate { get; set; } // Changed to DateOnly
        public string? ReasonForReturn { get; set; }
        public string? ReleasedBy { get; set; }
        public DateOnly? PullOutDate { get; set; } // PullOutDate is a date-only value (no time)
        public DateTime? PullOutDateStartAt { get; set; }
        public DateTime? PullOutDateEndAt { get; set; }
        public string? RequestStatus { get; set; }
        public string? TripTicketNumber { get; set; }
        public string? Driver { get; set; }
        public string? Helper { get; set; }
        public string? ReceivedBy { get; set; } // New property to capture who received the returned/picked up items
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? RequestedBy { get; set; }
        public long? MobileID { get; set; }
        public string? UpdatedBy { get; set; }

        [ForeignKey("RequestID")]
        public List<DocumentReferenceModel>? DocumentReference { get; set; }
        [ForeignKey("RequestID")]
        public SignatureModel? Signature { get; set; }
        [ForeignKey("RequestID")]
        public RemarksModel? Remarks { get; set; }
        [ForeignKey("FormCategoryID")]
        public CategoryModel? FormCategory { get; set; }
        [ForeignKey("ItemCategoryID")]
        public CategoryModel? ItemCategory { get; set; }
        [ForeignKey("ClientID")]
        public ACCMSTModel? Client { get; set; }
        [ForeignKey("MobileID")]
        public MobileModel? Mobile { get; set; }

    }
}
