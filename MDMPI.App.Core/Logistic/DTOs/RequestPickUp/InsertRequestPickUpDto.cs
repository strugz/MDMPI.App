using System;
using System.Collections.Generic;

namespace MDMPI.App.Core.Logistic.DTOs.RequestPickUp
{
    public class InsertRequestPickUpDto
    {
        public string? ClientID { get; set; }
        public long? ItemCategoryID { get; set; }

        /// <summary>
        /// All selected item categories. When provided, the first entry also
        /// fills the scalar ItemCategoryID column (primary category) so
        /// existing consumers keep working.
        /// </summary>
        public List<long>? ItemCategoryIDs { get; set; }
        public List<string>? DocumentReference { get; set; }
        public DateTime? DatePickUp { get; set; }
        public string? Status { get; set; } // default in DB is 'New Request' if not supplied
        public string? CreatedBy { get; set; }
    }
}
