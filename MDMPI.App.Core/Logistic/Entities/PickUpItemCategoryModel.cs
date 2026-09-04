using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MDMPI.App.Core.Logistic.Entities
{
    /// <summary>
    /// One selected item category on a Pick Up request. A request may carry
    /// several; the scalar ItemCategoryID column on a_tblRequestPickUpMDMPI
    /// keeps the first (primary) selection for backward compatibility.
    /// Mirrors the a_tblRequestDocumentReference child-table pattern.
    /// </summary>
    public class PickUpItemCategoryModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long PickUpItemCategoryID { get; set; }

        public long RequestID { get; set; }

        public long ItemCategoryID { get; set; }
    }
}
