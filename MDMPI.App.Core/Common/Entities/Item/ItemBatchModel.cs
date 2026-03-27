using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MDMPI.App.Core.Common.Entities.Item
{
    public class ItemBatchModel
    {
        // New per-batch identity key
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long RequestItemBatchID { get; set; }

        // FK to the item (must match RequestStandardItemModel.RequestItemID)
        public long RequestItemID { get; set; }

        public string? BatchSerial { get; set; }
        public decimal? BatchQuantity { get; set; }
        public DateOnly? ExpiryDate { get; set; }
    }
}