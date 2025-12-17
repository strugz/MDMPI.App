using MDMPI.App.Core.Common.Entities;
using MDMPI.App.Core.CommonOldEntities.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MDMPI.App.Core.Logistic.Entities
{
    public class RequestAirSeaModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long RequestID { get; set; }
        [MaxLength(100)]
        public string? ClientID { get; set; }
        public long? ItemCategoryID { get; set; }
        [MaxLength(100)]
        public string? ReceivedBy { get; set; }
        [MaxLength(100)]
        public string? WaybillNumber { get; set; }
        [MaxLength(100)]
        public string? TripTicketNumber { get; set; }
        [MaxLength(100)]
        public string? Driver { get; set; }
        [MaxLength(100)]
        public string? Helper { get; set; }
        public long? MobileID { get; set; }
        public DateTime? DatePickUp { get; set; }
        public DateTime? ItemPreparedAt { get; set; }
        public DateTime? ItemPreparedEndAt { get; set; }
        [MaxLength(4)]
        public string? PreparedBy { get; set; }
        [MaxLength(15)]
        public string? Status { get; set; }
        [MaxLength(255)]
        public string? Remarks { get; set; }
        [MaxLength(100)]
        public string? CreatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Enriched to support multiple document references
        /// </summary>
        [ForeignKey("RequestID")]
        public List<DocumentReferenceModel>? DocumentReference { get; set; }
        [ForeignKey("RequestID")]
        public SignatureModel? Signature { get; set; }
        [ForeignKey("ClientID")]
        public ACCMSTModel? Client { get; set; }
        [ForeignKey("RequestID")]
        public RemarksModel? CancelRemarks { get; set; }


    }
}
