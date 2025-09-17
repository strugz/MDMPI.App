using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDMPI.App.Core.CommonOldEntities.DTOs
{
    public class ACCMSTDto
    {
        public string? ACCMID { get; set; }        // Client ID
        public string? ACCMSC { get; set; }        // Short Code
        public string? ACCMNM { get; set; }        // Name
        public string? ACCMBC { get; set; }        // Business Code
        public string? ACCMAD { get; set; }        // Address
        public string? ACCMPH { get; set; }        // Phone
        public string? ACCMEM { get; set; }        // Email
        public string? ACCMWS { get; set; }        // Website
        public string? ACCSTS { get; set; }        // Status
        public string? ACCOWN { get; set; }        // Owner
    }
}
