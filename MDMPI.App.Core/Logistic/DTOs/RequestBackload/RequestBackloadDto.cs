using System;

namespace MDMPI.App.Core.Logistic.DTOs.RequestBackload
{
    public class RequestBackloadDto
    {
        public long? BackLoadID { get; set; }
        public long? RequestID { get; set; }
        public string? Remarks { get; set; }
        public DateTime? DateReported { get; set; }
    }
}
