using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MDMPI.App.Core.Common.Entities
{
    [Table("a_tblItemCounters")]
    public class ItemCounterModel
    {
        [Key]
        [Column(TypeName = "char(6)")]
        public string YearMonth { get; set; } = default!;
        public int LastNumber { get; set; }
    }
}