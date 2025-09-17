using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MDMPI.App.Core.Common.DTOs
{
    public class RemarksDto
    {
        [Key]
        [JsonPropertyName("RequestID")]
        public long? RequestID { get; set; }
        [JsonPropertyName("Remarks")]
        public string? Remarks { get; set; }
        [JsonPropertyName("Date")]
        public string? Date { get; set; }
    }
}
