using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDMPI.App.Core.Logistic.DTOs
{
    public class InsertRequestDto
    {
        public string? RequestClientID { get; set; }
        public string? RequestShippingMethod { get; set; } = string.Empty;
        public string? RequestDeliveryTerms { get; set; } = string.Empty;
        public string? RequestDeliveryDate { get; set; }
        public string? RequestPreference { get; set; } = string.Empty;
        public string? RequestStatus { get; set; } = string.Empty;
        public string? RequestBy { get; set; } = string.Empty;
        public string? RequestCreatedBy { get; set; } = string.Empty;
        public List<string>? DocumentReference { get; set; }
    }
}
