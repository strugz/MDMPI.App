using MDMPI.App.Core.Common.Entities;
using MDMPI.App.Core.Common.Entities;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MDMPI.App.Core.Logistic.Entities
{
    [Table("a_tblRequestPickUpMDMPI", Schema = "dbo")]
    public class RequestPickUpModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long RequestID { get; set; }

        [MaxLength(150)]
        public string? ClientID { get; set; }

        public long? ItemCategoryID { get; set; }

        [MaxLength(100)]
        public string? PreparedBy { get; set; }

        public DateTime? ItemPreparedAt { get; set; }
        public DateTime? ItemPreparedEndAt { get; set; }
        public DateTime? DatePickUp { get; set; }

        [MaxLength(255)]
        public string? Remarks { get; set; }

        [MaxLength(50)]
        public string? Status { get; set; }

        [MaxLength(100)]
        public string? ReleasedBy { get; set; }

        [MaxLength(100)]
        public string? ReceivedBy { get; set; }

        [MaxLength(100)]
        public string? CreatedBy { get; set; }

        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }

        // Navigation properties
        [ForeignKey("RequestID")]
        public List<DocumentReferenceModel>? DocumentReference { get; set; }
        [ForeignKey("ItemCategoryID")]
        public CategoryModel? ItemCategory { get; set; }

        [ForeignKey("ClientID")]
        public ACCMSTModel? Client { get; set; }
    }
}
