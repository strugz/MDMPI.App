using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MDMPI.App.Core.Common.DTOs
{
    public class MobileDto
    {
        [JsonPropertyName("MobileID")]
        public long MobileID { get; set; }
        [JsonPropertyName("MobileName")]
        public string? MobileName { get; set; }
    }
}
