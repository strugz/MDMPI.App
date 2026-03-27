using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace MDMPI.App.Core.Common.Entities.Item
{
    public class ItemModel
    {
        // New per-item identity key
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long RequestItemID { get; set; }
        public long RequestID { get; set; }

        public string? ItemCode { get; set; }
        public string? Description { get; set; }
        public decimal? Qty { get; set; }
        public string? Unit { get; set; }

        [ForeignKey("RequestItemID")]
        public List<ItemBatchModel>? Batch { get; set; }
    }
}