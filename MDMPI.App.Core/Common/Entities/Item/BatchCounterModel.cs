using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MDMPI.App.Core.Common.Entities.Item
{
    [Table("a_tblBatchCounters")]
    public class BatchCounterModel
    {
        [Key]
        [Column(TypeName = "char(6)")]
        public string YearMonth { get; set; } = default!;
        public int LastNumber { get; set; }
    }
}