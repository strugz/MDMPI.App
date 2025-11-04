using System;
using System.Collections.Generic;

namespace MDMPI.App.Core.Logistic.DTOs.RequestPickUp
{
    public class InsertRequestPickUpDto
    {
        public string? ClientID { get; set; }
        public long? ItemCategoryID { get; set; }
        public List<string>? DocumentReference { get; set; }
        public DateTime? DatePickUp { get; set; }
        public string? Status { get; set; } // default in DB is 'New Request' if not supplied
    }
}
