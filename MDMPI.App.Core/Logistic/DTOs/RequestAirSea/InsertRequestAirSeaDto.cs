using System;
using System.Collections.Generic;

namespace MDMPI.App.Core.Logistic.DTOs.RequestAirSea
{
    public class InsertRequestAirSeaDto
    {
        public long? ItemCategoryID { get; set; }
        public string? ClientID { get; set; }
        public List<string>? DocumentReference { get; set; }
        public DateTime? DatePickUp { get; set; }
        public string? Status { get; set; }
    }
}
