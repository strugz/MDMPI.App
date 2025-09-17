using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDMPI.App.Core.Common.DTOs
{
    public class DocumentReferenceDto
    {
        public long RequestID { get; set; }
        public string Reference { get; set; } = string.Empty;
    }
}
