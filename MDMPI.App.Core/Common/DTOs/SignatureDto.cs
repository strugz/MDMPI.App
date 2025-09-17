using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDMPI.App.Core.Common.DTOs
{
    public class SignatureDto
    {
        [Key]
        public long? RequestID { get; set; }
        public string? Image { get; set; }
    }
}
