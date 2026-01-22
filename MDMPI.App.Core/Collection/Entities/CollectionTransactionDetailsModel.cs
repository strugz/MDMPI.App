using MDMPI.App.Core.CommonOldEntities.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MDMPI.App.Core.Collection.Entities
{
    public class CollectionTransactionDetailsModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long ID { get; set; }

        [Required]
        [MaxLength(16)]
        public string ReferenceCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string ClientID { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string CollectorID { get; set; } = string.Empty;

        [Required]
        public DateTime CollectionDate { get; set; }

        [Required]
        [MaxLength(20)]
        public string VisitType { get; set; } = string.Empty;

        [MaxLength(15)]
        public string? Bank { get; set; }

        [MaxLength(25)]
        public string? CheckNo { get; set; }

        public DateTime? CheckDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? Amount { get; set; }

        [MaxLength(25)]
        public string? SalesInvoiceReferenceForPayment { get; set; }

        public string? SummaryOfVisit { get; set; }

        public DateTime? created_at { get; set; }

        public DateTime? updated_at { get; set; }
        
        [MaxLength(20)]
        public string? Status { get; set; }

        [ForeignKey("ClientID")]
        public ACCMSTModel? Client { get; set; }
    }
}
