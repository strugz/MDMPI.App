using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MDMPI.App.Core.Logistic.Entities
{
    /// <summary>
    /// An item that was NOT actually pulled out (lost / not found) on a
    /// Pull Out / Return request, with the courier's remarks. Items absent
    /// from this table are considered included in the pull out.
    /// Identified by RequestID + ItemCode.
    /// </summary>
    public class LoseItemModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long LoseItemID { get; set; }

        public long RequestID { get; set; }

        public string? ItemCode { get; set; }

        public string? Remarks { get; set; }

        public DateTime? CreatedAt { get; set; }
    }
}
