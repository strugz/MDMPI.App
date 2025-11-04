using MDMPI.App.Core.Common.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace MDMPI.App.Core.Logistic.DTOs.RequestAirSea
{
    public class UpdateRequestAirSeaDto
    {
        public long RequestID { get; set; }
        public long? MobileID { get; set; }
        public string? RiderName { get; set; }
        public DateTime? ItemPreparedAt { get; set; }
        public DateTime? ItemPreparedEndAt { get; set; }
        public string? PreparedBy { get; set; }
        public string? Status { get; set; }
        public string? Remarks { get; set; }
    }
}
